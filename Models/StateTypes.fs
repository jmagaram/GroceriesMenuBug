﻿module Models.StateTypes

open System
open SynchronizationTypes

[<Measure>]
type days

[<Struct>]
type ItemId = ItemId of Guid

[<Struct>]
type ItemName = ItemName of string

[<Struct>]
type Note = Note of string

[<Struct>]
type Quantity = Quantity of string

type Repeat =
    { Interval: int<days>
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

type Item =
    { ItemId: ItemId
      ItemName: ItemName
      Note: Note option
      Quantity: Quantity option
      CategoryId: CategoryId option
      Schedule: Schedule }
    interface IKey<ItemId> with
        member this.Key = this.ItemId

type Store =
    { StoreId: StoreId
      StoreName: StoreName }
    interface IKey<StoreId> with
        member this.Key = this.StoreId

type Category =
    { CategoryId: CategoryId
      CategoryName: CategoryName }
    interface IKey<CategoryId> with
        member this.Key = this.CategoryId

type NotSoldItem =
    { StoreId: StoreId
      ItemId: ItemId }
    interface IKey<NotSoldItem> with
        member this.Key = this

type NotSoldCategory =
    { StoreId: StoreId
      CategoryId: CategoryId }
    interface IKey<NotSoldCategory> with
        member this.Key = this

// include "show empty categories"
type Settings = 
    { StoreFilter: StoreId option }
    interface IKey<string> with
        member this.Key = "singleton"

type ItemsTable = DataTable<ItemId, Item>
type StoresTable = DataTable<StoreId, Store>
type CategoryTable = DataTable<CategoryId, Category>
type NotSoldItemTable = DataTable<NotSoldItem, NotSoldItem>
type NotSoldCategoryTable = DataTable<NotSoldCategory, NotSoldCategory>
type SettingsRow = DataRow<Settings>

type State =
    { Items: ItemsTable
      Categories: CategoryTable
      Stores: StoresTable
      NotSoldItems: NotSoldItemTable
      NotSoldCategories : NotSoldCategoryTable
      Settings: SettingsRow }

type CategoryReference =
    | ExistingCategory of CategoryId
    | NewCategory of Category

type StoreReference =
    | ExistingStore of StoreId
    | NewStore of Store

type ItemUpsert =
    { ItemId: ItemId
      ItemName: ItemName
      Note: Note option
      Quantity: Quantity option
      Category: CategoryReference option
      Schedule: Schedule 
      NotSoldAt : StoreReference list }
// list each store
// don't need new ones
// for each store...
// available, not available, category not available

type StoreUpsert =
    { StoreId : StoreId
      StoreName : StoreName
      UnstockedCategories : Set<CategoryId>
      UnstockedItems : Set<ItemId> }

type ItemMessage =
    | InsertItem of ItemUpsert
    | UpdateItem of ItemUpsert
    | DeleteItem of ItemId

type CategoryMessage = 
    | InsertCategory of Category
    | DeleteCategory of CategoryId

type StoreMessage = 
    | InsertStore of Store
    | DeleteStore of StoreId

type ShoppingListMessage =
    | ClearStoreFilter
    | SetStoreFilterTo of StoreId

type StateMessage =
    | ItemMessage of ItemMessage
    | StoreMessage of StoreMessage
    | CategoryMessage of CategoryMessage
    | ShoppingListMessage of ShoppingListMessage

