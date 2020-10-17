﻿module Models.StateTypes

open System
open ChangeTrackerTypes

[<Struct>]
type ItemId = ItemId of Guid

[<Struct>]
type ItemName = ItemName of string

[<Struct>]
type Note = Note of string

[<Struct>]
type Quantity = Quantity of string

[<Struct>]
type Frequency = Frequency of int<days>

type Repeat =
    { Frequency: Frequency
      PostponedUntil: DateTimeOffset option }

type Schedule =
    | Completed
    | Once
    | Repeat of Repeat

[<Struct>]
type CategoryId = CategoryId of Guid

[<Struct>]
type CategoryName = CategoryName of string

[<Struct>]
type StoreId = StoreId of Guid

[<Struct>]
type StoreName = StoreName of string

type Etag = Etag of string

type Item =
    { ItemId: ItemId
      ItemName: ItemName
      Etag: Etag option
      Note: Note option
      Quantity: Quantity option
      CategoryId: CategoryId option
      Schedule: Schedule }
    interface IKey<ItemId> with
        member this.Key = this.ItemId

type Store =
    { StoreId: StoreId
      StoreName: StoreName
      Etag: Etag option }
    interface IKey<StoreId> with
        member this.Key = this.StoreId

type Category =
    { CategoryId: CategoryId
      CategoryName: CategoryName
      Etag: Etag option }
    interface IKey<CategoryId> with
        member this.Key = this.CategoryId

type NotSoldItem =
    { StoreId: StoreId
      ItemId: ItemId }
    interface IKey<NotSoldItem> with
        member this.Key = this

type TextBox =
    { ValueTyping: string
      ValueCommitted: string }

type TextBoxMessage =
    | TypeText of string
    | LoseFocus

type SearchTerm = SearchTerm of string

type ShoppingListSettings =
    { StoreFilter: StoreId option
      PostponedViewHorizon: int<days>
      HideCompletedItems: bool
      ItemTextFilter: SearchTerm option }
    interface IKey<string> with
        member this.Key = "singleton"

type ItemsTable = DataTable<ItemId, Item>
type StoresTable = DataTable<StoreId, Store>
type CategoryTable = DataTable<CategoryId, Category>
type NotSoldItemTable = DataTable<NotSoldItem, NotSoldItem>
type ShoppingListSettingsRow = DataRow<ShoppingListSettings>

type ScheduleKind =
    | Once
    | Completed
    | Repeat

type CategoryMode =
    | ChooseExisting
    | CreateNew

type ItemAvailability = { Store: Store; IsSold: bool }

// If this was a generic add-on to State, maybe could define this and it's
// extension methods in one file (using implicit extensions) later in the
// dependency order.
type ItemForm =
    { ItemId: ItemId option
      ItemName : TextBox
      Etag: Etag option
      Quantity: TextBox
      Note: TextBox
      ScheduleKind: ScheduleKind
      Frequency: Frequency
      Postpone: int<days> option
      CategoryMode: CategoryMode
      NewCategoryName: TextBox
      CategoryChoice: Category option
      CategoryChoiceList: Category list
      Stores: ItemAvailability list }

type ItemFormResult =
    { Item: Item
      InsertCategory: Category option
      NotSold: StoreId list }

type SerializedId = string

type CategoryEditForm =
    { CategoryId: CategoryId option
      CategoryName: TextBox
      Etag: Etag option }

type CategoryEditFormResult =
    | InsertCategory of CategoryName
    | EditCategory of Category

type CategoryEditFormMessage = CategoryNameMessage of TextBoxMessage

type CategoryEditPageMessage =
    | BeginEditCategory of SerializedId
    | BeginCreateNewCategory
    | CategoryEditFormMessage of CategoryEditFormMessage
    | SubmitCategoryEditForm
    | CancelCategoryEditForm
    | DeleteCategory

type StoreEditForm =
    { StoreId: StoreId option
      StoreName: TextBox
      Etag: Etag option }

type StoreEditFormResult =
    | InsertStore of StoreName
    | EditStore of Store

// Not sure these message/command types need to be displayed here; maybe later
// in the project dependency order
type StoreEditFormMessage = StoreNameMessage of TextBoxMessage

type StoreEditPageMessage =
    | BeginEditStore of SerializedId
    | BeginCreateNewStore
    | StoreEditFormMessage of StoreEditFormMessage
    | SubmitStoreEditForm
    | CancelStoreEditForm
    | DeleteStore

type ItemEditPageMessage =
    | BeginEditItem of SerializedId
    | BeginCreateNewItem
    | BeginCreateNewItemWithName of string
    | ItemEditFormMessage of ItemFormMessage
    | SubmitItemEditForm
    | CancelItemEditForm
    | DeleteItem

// Make sure all these are pure messages; include current time and any necessary
// GUID values; don't create these on-the-fly inside various methods Maybe there
// should be a standard "ItemFormHost" message that includes delete, create,
// cancel, submit and then use this wherever the form is hosted rather than
// hardwiring it into the ItemPageMessage and ItemPopUpDilaogEditorMessage etc.
and ItemFormMessage =
    | ItemNameSet of string
    | ItemNameBlur
    | QuantitySet of string
    | QuantityBlur
    | NoteSet of string
    | NoteBlur
    | ScheduleOnce
    | ScheduleCompleted
    | ScheduleRepeat
    | FrequencySet of int<days>
    | PostponeSet of int<days>
    | PostponeClear
    | CategoryModeChooseExisting
    | CategoryModeCreateNew
    | ChooseCategoryUncategorized
    | ChooseCategory of Guid
    | NewCategoryNameSet of string
    | NewCategoryNameBlur
    | StoresSetAvailability of store: StoreId * isSold: bool
    | Purchased
    | Transaction of ItemFormMessage seq

type State =
    { Items: ItemsTable
      Categories: CategoryTable
      Stores: StoresTable
      NotSoldItems: NotSoldItemTable
      ShoppingListSettings: ShoppingListSettingsRow
      LastCosmosTimestamp: int option
      ItemEditForm: ItemForm option
      CategoryEditPage: CategoryEditForm option 
      StoreEditPage : StoreEditForm option 
      ItemEditPage : ItemForm option }

type ImportChanges =
    { ItemChanges: Change<Item, ItemId> list
      CategoryChanges: Change<Category, CategoryId> list
      StoreChanges: Change<Store, StoreId> list
      NotSoldItemChanges: Change<NotSoldItem, NotSoldItem> list
      LatestTimestamp: int option }

type ItemMessage =
    | MarkComplete of ItemId
    | BuyAgain of ItemId
    | RemovePostpone of ItemId
    | Postpone of ItemId * int<days>
    | InsertItem of Item
    | UpdateItem of Item
    | UpsertItem of Item
    | DeleteItem of ItemId

type ShoppingListSettingsMessage =
    | ClearStoreFilter
    | SetStoreFilterTo of StoreId
    | SetPostponedViewHorizon of int<days>
    | HideCompletedItems of bool
    | SetItemFilter of string
    | ClearItemFilter

type StateMessage =
    | ItemEditPageMessage of ItemEditPageMessage
    | CategoryEditPageMessage of CategoryEditPageMessage
    | StoreEditPageMessage of StoreEditPageMessage
    | ItemMessage of ItemMessage
    | ShoppingListSettingsMessage of ShoppingListSettingsMessage
    | Import of ImportChanges
    | AcceptAllChanges
    | ResetToSampleData
