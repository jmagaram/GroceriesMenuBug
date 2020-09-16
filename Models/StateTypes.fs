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

type ShoppingListViewOptions = 
    { StoreFilter: StoreId option }
    interface IKey<string> with
        member this.Key = "singleton"

type ItemTable = DataTable<ItemId, Item>
type StoreTable = DataTable<StoreId, Store>
type CategoryTable = DataTable<CategoryId, Category>
type NotSoldItemsTable = DataTable<NotSoldItem, NotSoldItem>
type ShoppingListViewOptionsRow = DataRow<ShoppingListViewOptions>

type State =
    { Items: ItemTable
      Categories: CategoryTable
      Stores: StoreTable
      NotSoldItems: NotSoldItemsTable
      ShoppingListViewOptions: ShoppingListViewOptionsRow }

type ShoppingListMessage =
    | ClearStoreFilter
    | SetStoreFilterTo of StoreId

type StateMessage =
    | DeleteStore of StoreId
    | DeleteCategory of CategoryId
    | DeleteItem of ItemId
    | ShoppingListMessage of ShoppingListMessage
