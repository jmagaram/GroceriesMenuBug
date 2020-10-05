﻿using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Pages {
    public partial class Sync : ComponentBase, IDisposable {
        private static readonly string EndpointUri = "https://localhost:8081";
        private static readonly string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private CosmosClient _client;
        private Database _database;
        private Container _container;
        private const string _applicationName = "CosmosDBDotnetQuickstart";
        private readonly string _databaseId = "db";
        private readonly string _containerId = "items";
        private string _userId = "justin@magaram.com"; // Varies by customer; the partition key
        private const string _partitionKeyPath = "/UserId";

        [Inject]
        public Data.ApplicationStateService StateService { get; set; }

        private void LogMessage(string s) {
            Log.Insert(0, s);
        }

        public List<string> Log { get; } = new List<string>();

        protected override async Task OnInitializedAsync() {
            StatusMessage = "Initializing...";
            _client = CreateClient();
            await CreateDatabaseIfNotExistsAsync();
            await CreateContainerIfNotExistsAsync();
            StatusMessage = "";
        }

        private async Task ResetToEmpty() {
            LogMessage("Resetting...");
            StatusMessage = "Resetting...";
            await CreateDatabaseIfNotExistsAsync();
            await DeleteDatabaseAsync();
            await CreateDatabaseIfNotExistsAsync();
            await CreateContainerIfNotExistsAsync();
            StatusMessage = "";
        }

        private static CosmosClient CreateClient() =>
            new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = _applicationName });

        private async Task CreateDatabaseIfNotExistsAsync() =>
            _database = await _client.CreateDatabaseIfNotExistsAsync(_databaseId);

        private async Task CreateContainerIfNotExistsAsync() =>
            _container = await _database.CreateContainerIfNotExistsAsync(_containerId, _partitionKeyPath, 400);

        protected async Task PushAsync() {
            StatusMessage = "Pushing...";
            var changes = Models.Dto.pushChanges(_userId, StateService.Current);
            LogMessage($"PUSH items:{changes.Items.Count} cats:{changes.Categories.Count} stores:{changes.Stores.Count}");
            var partitionKey = new PartitionKey(_userId);
            foreach (var i in changes.Items) {
                await _container.UpsertItemAsync(i, partitionKey, new ItemRequestOptions { IfMatchEtag = i.Etag });
            }
            foreach (var i in changes.Categories) {
                await _container.UpsertItemAsync(i, partitionKey, new ItemRequestOptions { IfMatchEtag = i.Etag });
            }
            foreach (var i in changes.Stores) {
                await _container.UpsertItemAsync(i, partitionKey, new ItemRequestOptions { IfMatchEtag = i.Etag });
            }
            foreach (var i in changes.NotSoldItems) {
                await _container.UpsertItemAsync(i, partitionKey, new ItemRequestOptions { IfMatchEtag = i.Etag });
            }
            StatusMessage = "";
            LogMessage("PUSH finish");
            //StateService.Update(StateTypes.StateMessage.AcceptAllChanges);
        }

        protected async Task PullAsync() {
            var ts = StateService.LastCosmosSyncTimestamp;
            var items = await Changes<DtoTypes.Item>(_userId, ts, DtoTypes.DocumentKind.Item);
            var categories = await Changes<DtoTypes.Category>(_userId, ts, DtoTypes.DocumentKind.Category);
            var stores = await Changes<DtoTypes.Store>(_userId, ts, DtoTypes.DocumentKind.Store);
            var notSoldItems = await Changes<DtoTypes.NotSoldItem>(_userId, ts, DtoTypes.DocumentKind.NotSoldItem);
            var newTimestamps =
                items.Select(i => i.Timestamp)
                .Concat(categories.Select(i => i.Timestamp))
                .Concat(stores.Select(i => i.Timestamp))
                .Concat(notSoldItems.Select(i => i.Timestamp))
                .ToList();
            var pull = Dto.pull(items, categories, stores, notSoldItems);
            LogMessage($"PULL items: {items.Count} cats: {categories.Count} stores: {stores.Count} notSold : {notSoldItems.Count}");
            var s = Dto.processPull(pull, StateService.Current);
            StateService.ReplaceState(s);
            if (newTimestamps.Count > 0) {
                StateService.LastCosmosSyncTimestamp = newTimestamps.Max();
            }
            LogMessage($"PULL done");
        }

        protected async Task DeleteDatabaseAsync() => await _database.DeleteAsync();

        private async Task ReplaceItemAsync(string itemId) {
            var partitionKey = new PartitionKey(_userId);
            ItemResponse<DtoTypes.Item> item = await _container.ReadItemAsync<DtoTypes.Item>(itemId.ToString(), partitionKey);
            var itemBody = item.Resource;
            itemBody.Note = itemBody.Note + $" updated {DateTime.Now}";
            item = await _container.ReplaceItemAsync(itemBody, itemBody.ItemId.ToString(), partitionKey);
        }

        private async Task QueryItemsAsync() => await QueryItemsAsyncOfType<Models.DtoTypes.Item>();

        private async Task QueryItemsAsyncOfType<T>() {
            var sqlQueryText = "SELECT * FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            var queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition);
            var docs = new List<T>();
            while (queryResultSetIterator.HasMoreResults) {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var doc in currentResultSet) {
                    docs.Add(doc);
                }
            }
        }

        private string QueryText(string userId, int documentKind, int timestamp) =>
            $"SELECT * FROM c WHERE c._ts > {timestamp} AND c.ItemName = {_userId} AND c.DocumentKind = {documentKind}";

        private async Task<List<T>> Changes<T>(string userId, int timestamp, DtoTypes.DocumentKind documentKind) where T : Models.DtoTypes.GroceryDocument {
            var query = _container.GetItemLinqQueryable<T>()
                .Where(i => i.Timestamp > timestamp)
                .Where(i => i.UserId == _userId)
                .Where(i => i.DocumentKind == documentKind);
            var docs = new List<T>();
            using (var iterator = query.ToFeedIterator()) {
                while (iterator.HasMoreResults) {
                    foreach (var item in await iterator.ReadNextAsync()) {
                        docs.Add(item);
                    }
                }
            }
            return docs;
        }

        //private async Task GetRecentChanges() {
        //    var ts = StateService.LastCosmosSyncTimestamp;
        //    var items = await Changes<DtoTypes.Item>(_userId, ts, DtoTypes.DocumentKind.Item);
        //    var categories = await Changes<DtoTypes.Category>(_userId, ts, DtoTypes.DocumentKind.Category);
        //    var stores = await Changes<DtoTypes.Store>(_userId, ts, DtoTypes.DocumentKind.Store);
        //    var notSoldItems = await Changes<DtoTypes.NotSoldItem>(_userId, ts, DtoTypes.DocumentKind.NotSoldItem);
        //    var newTimestamps =
        //        items.Select(i => i.Timestamp)
        //        .Concat(categories.Select(i => i.Timestamp))
        //        .Concat(stores.Select(i => i.Timestamp))
        //        .Concat(notSoldItems.Select(i => i.Timestamp))
        //        .ToList();
        //    if (newTimestamps.Count > 0) {
        //        StateService.LastCosmosSyncTimestamp = newTimestamps.Max();
        //    }
        //    var changes = new DtoTypes.PushChanges { Items = items, Categories = categories, Stores = stores, NotSoldItems = notSoldItems };
        //    LogMessage($"Items: {items.Count} Cats: {categories.Count} Stores: {stores.Count} NotSold : {notSoldItems.Count}");
        //}

        //var sqlQueryText = $"SELECT * FROM c WHERE c._ts > {StateService.LastCosmosSyncTimestamp} AND c.ItemName = {_userId} AND c.DocumentKind = {DtoTypes.DocumentKind.Item} ";
        //QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        //var queryResultSetIterator = _container.GetItemQueryIterator<DtoTypes.GroceryDocument>(queryDefinition);
        //var docs = new List<DtoTypes.GroceryDocument>();
        //while (queryResultSetIterator.HasMoreResults) {
        //    var currentResultSet = await queryResultSetIterator.ReadNextAsync();
        //    foreach (var doc in currentResultSet) {
        //        docs.Add(doc);
        //    }
        //}

        protected bool IsReady => StatusMessage == "";

        protected string StatusMessage { get; set; } = "";

        public void Dispose() => _client.Dispose();

        //    var queryable = container
        //.GetItemLinqQueryable<IDictionary<string, object>>();
        //    var oneDay = DateTime.UtcNow.AddDays(-1);
        //    var query = queryable
        //        .OrderByDescending(s => s["timestamp"])
        //        .Where(s => (DateTime)s["timestamp"] > oneDay);
        //    var iterator = query.ToFeedIterator();

        //private async Task QueryItemsAsync() {
        //    var sqlQueryText = "SELECT * FROM c";
        //    QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

        //    FeedIterator<DtoTypes.GroceryDocument> queryResultSetIterator = _container.GetItemQueryIterator<DtoTypes.GroceryDocument>(queryDefinition);
        //    List<DtoTypes.GroceryDocument> docs = new List<DtoTypes.GroceryDocument>();
        //    while (queryResultSetIterator.HasMoreResults) {
        //        FeedResponse<DtoTypes.GroceryDocument> currentResultSet = await queryResultSetIterator.ReadNextAsync();
        //        foreach (DtoTypes.GroceryDocument doc in currentResultSet) {
        //            docs.Add(doc); // does not work; returns just base class
        //        }
        //    }
        //}

        // Querying a document AND mapping to specific type
        ////https://www.annytab.com/safe-update-in-cosmos-db-with-etag-asp-net-core/

        // https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos.Samples/Usage/Queries/Program.cs#L154-L186


        //private async Task AddItemsOfType<T>(PartitionKey partitionKey, IEnumerable<T> items, Func<T, string> id) {
        //    foreach (var i in items) {
        //        try {
        //            ItemResponse<T> doc = await _container.ReadItemAsync<T>(id(i), partitionKey);
        //        }
        //        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound) {
        //            ItemResponse<T> doc = await _container.CreateItemAsync(i, partitionKey);
        //        }
        //    }
        //}
    }
}