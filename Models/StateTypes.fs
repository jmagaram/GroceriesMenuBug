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

type ItemMessage =
    | InsertItem of Item
    | UpdateItem of Item
    | DeleteItem of ItemId

type CategoryMessage = 
    | InsertCategory of Category
    | DeleteCategory of CategoryId

type StoreMessage = 
    | InsertStore of Store
    | DeleteStore of StoreId

type NotSoldItemMessage =
    | InsertNotSoldItem of NotSoldItem
    | DeleteNotSoldItem of NotSoldItem

type NotSoldCategoryMessage =
    | InsertNotSoldCategory of NotSoldCategory
    | DeleteNotSoldCategory of NotSoldCategory

type SettingsMessage =
    | ClearStoreFilter
    | SetStoreFilterTo of StoreId

type StateMessage =
    | ItemMessage of ItemMessage
    | StoreMessage of StoreMessage
    | CategoryMessage of CategoryMessage
    | NotSoldItemMessage of NotSoldItemMessage 
    | NotSoldCategoryMessage of NotSoldCategoryMessage
    | SettingsMessage of SettingsMessage
    | Transaction of StateMessage seq
