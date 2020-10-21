﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using Models;

using WebApp.Common;
using WebApp.Data;

using static Models.CoreTypes;

using ItemMessage = Models.ItemModule.Message;
using SettingsMessage = Models.ShoppingListSettingsModule.Message;
using StateItemMessage = Models.StateTypes.ItemMessage;
using StateMessage = Models.StateTypes.StateMessage;

namespace WebApp.Pages {
    public enum SyncStatus { SynchronizingNow, NoChangesToPush, ShouldSync }

    public partial class ShoppingList : ComponentBase, IDisposable {
        CompositeDisposable _disposables;

        string _textFilter;
        readonly BehaviorSubject<string> _textFilterTyped = new BehaviorSubject<string>("");

        [Inject]
        public Models.Service StateService { get; set; }

        [Inject]
        NavigationManager Navigation { get; set; }

        [Inject]
        public CosmosConnector Cosmos { get; set; }

        public SyncStatus SyncStatus { get; set; } = SyncStatus.NoChangesToPush;

        protected override void OnInitialized() {
            base.OnInitialized();
            OnTextFilterClear();
            _disposables = new CompositeDisposable
            {
                UpdateItems(),
                UpdateStoreFilterPickerList(),
                UpdateStoreFilterSelectedItem(),
                UpdateTextFilter(),
                UpdateCanSync(),
                ProcessTextFilterTyped()
            };
        }

        private IDisposable ProcessTextFilterTyped() =>
            _textFilterTyped
            .DistinctUntilChanged()
            .Throttle(TimeSpan.FromSeconds(0.75))
            .Subscribe(s =>
            {
                StateService.Update(StateMessage.NewShoppingListSettingsMessage(SettingsMessage.NewSetItemFilter(s)));
                InvokeAsync(() => StateHasChanged());
            });

        private IDisposable UpdateCanSync() =>
            StateService.ShoppingList
            .Select(i => i.HasChanges)
            .Subscribe(async hasChanges =>
            {
                if (hasChanges) {
                    if (SyncStatus != SyncStatus.SynchronizingNow) {
                        SyncStatus = SyncStatus.SynchronizingNow;
                        await InvokeAsync(() => StateHasChanged());
                    }
                    try {
                        await OnSync();
                    }
                    catch {
                        SyncStatus = SyncStatus.ShouldSync;
                        await InvokeAsync(() => StateHasChanged());
                        await Task.Delay(TimeSpan.FromSeconds(0.5));
                    }
                }
                else {
                    if (SyncStatus != SyncStatus.NoChangesToPush) {
                        SyncStatus = SyncStatus.NoChangesToPush;
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        await InvokeAsync(() => StateHasChanged());
                    }
                }
            });

        private IDisposable UpdateStoreFilterSelectedItem() =>
            StateService.ShoppingList
            .Select(i => i.StoreFilter)
            .DistinctUntilChanged()
            .Subscribe(s => StoreFilter = s.IsNone() ? Guid.Empty : s.Value.StoreId.Item);

        private IDisposable UpdateTextFilter() =>
            StateService.ShoppingList
            .Select(i => i.SearchTerm)
            .DistinctUntilChanged()
            .Subscribe(s =>
            {
                TextFilter = s.IsNone() ? "" : s.Value.Item;
            });

        private IDisposable UpdateStoreFilterPickerList() =>
            StateService.ShoppingList
            .Select(i => i.Stores)
            .DistinctUntilChanged()
            .Subscribe(s => StoreFilterChoices = s.OrderBy(i => i.StoreName).ToList());

        private IDisposable UpdateItems() =>
            StateService
            .ShoppingList
            .Select(i => i.Items)
            .DistinctUntilChanged()
            .Subscribe(i => Items = i.ToList());

        private void OnNavigateToCategory(CategoryId id) {
            string categoryId = CategoryIdModule.serialize(id);
            Navigation.NavigateTo($"categoryedit/{categoryId}");
        }

        private async Task OnSync() {
            await Cosmos.CreateDatabaseAsync();
            await StateService.PushRequest().DoAsync(c => Cosmos.PushAsync(c));
            var state = StateService.Current;
            var changes =
                state.LastCosmosTimestamp.IsSome()
                ? await Cosmos.PullSinceAsync(state.LastCosmosTimestamp.Value)
                : await Cosmos.PullEverythingAsync();
            var import = Dto.pullResponse(changes.Items, changes.Categories, changes.Stores, changes.NotSoldItems);
            var msg = StateMessage.NewImport(import);
            StateService.Update(msg);
        }

        private void OnStoreFilterClear() =>
            StateService.Update(StateMessage.NewShoppingListSettingsMessage(SettingsMessage.ClearStoreFilter));

        private void OnStoreFilter(StoreId id) =>
            StateService.Update(StateMessage.NewShoppingListSettingsMessage(SettingsMessage.NewSetStoreFilterTo(id)));

        private void OnClickDelete(ItemId itemId) {
            var stateItemMessage = StateItemMessage.NewDeleteItem(itemId);
            var stateMessage = StateMessage.NewItemMessage(stateItemMessage);
            StateService.Update(stateMessage);
        }

        private void OnClickComplete(ItemId itemId) {
            var itemMessage = ItemMessage.MarkComplete;
            var stateItemMessage = StateItemMessage.NewModifyItem(itemId, itemMessage);
            var stateMessage = StateMessage.NewItemMessage(stateItemMessage);
            StateService.Update(stateMessage);
        }

        private void OnClickBuyAgain(ItemId itemId) {
            var itemMessage = ItemMessage.BuyAgain;
            var stateItemMessage = StateItemMessage.NewModifyItem(itemId, itemMessage);
            var stateMessage = StateMessage.NewItemMessage(stateItemMessage);
            StateService.Update(stateMessage);
        }

        private void OnClickRemovePostpone(ItemId itemId) {
            var itemMessage = ItemMessage.RemovePostpone;
            var stateItemMessage = StateItemMessage.NewModifyItem(itemId, itemMessage);
            var stateMessage = StateMessage.NewItemMessage(stateItemMessage);
            StateService.Update(stateMessage);
        }

        private void OnClickPostpone((ItemId itemId, int days) i) {
            var itemMessage = ItemMessage.NewPostpone(i.days);
            var stateItemMessage = StateItemMessage.NewModifyItem(i.itemId, itemMessage);
            var stateMessage = StateMessage.NewItemMessage(stateItemMessage);
            StateService.Update(stateMessage);
        }

        private void OnClickHideCompletedItems() =>
            StateService.Update(StateMessage.NewShoppingListSettingsMessage(SettingsMessage.NewHideCompletedItems(true)));

        private void OnClickShowCompletedItems() =>
            StateService.Update(StateMessage.NewShoppingListSettingsMessage(SettingsMessage.NewHideCompletedItems(false)));

        private void ShowPostponedWithinNext(int days) {
            StateService.Update(StateMessage.NewShoppingListSettingsMessage(SettingsMessage.NewSetPostponedViewHorizon(days)));
        }

        protected void OnTextFilterChange(ChangeEventArgs e) {
            string valueTyped = (string)e.Value;
            _textFilterTyped.OnNext(valueTyped);
        }

        protected void OnTextFilterClear() =>
            StateService.Update(StateMessage.NewShoppingListSettingsMessage(SettingsMessage.ClearItemFilter));

        protected void OnTextFilterKeyDown(KeyboardEventArgs e) {
            if (e.Key == "Escape") {
                OnTextFilterClear();
            }
        }

        protected void OnTextFilterBlur(FocusEventArgs e) { }

        protected string TextFilter
        {
            get { return _textFilter; }
            set
            {
                _textFilter = value;
                _textFilterTyped.OnNext(value);
            }
        }

        protected Guid StoreFilter { get; private set; }

        protected List<ShoppingListModule.Item> Items { get; private set; }

        protected List<Store> StoreFilterChoices { get; private set; }

        public void Dispose() => _disposables.Dispose();
    }
}
