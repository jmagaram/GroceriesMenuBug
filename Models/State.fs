﻿[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Models.State

open System
open ChangeTrackerTypes
open CoreTypes
open StateTypes

let categoriesTable (s: State) = s.Categories
let storesTable (s: State) = s.Stores
let itemsTable (s: State) = s.Items
let notSoldTable (s: State) = s.NotSoldItems
let shoppingListSettingsRow (s: State) = s.ShoppingListSettings

let shoppingListSettings s =
    s
    |> shoppingListSettingsRow
    |> DataRow.current
    |> Option.get

let categories = categoriesTable >> DataTable.current
let stores = storesTable >> DataTable.current
let items = itemsTable >> DataTable.current
let notSold = notSoldTable >> DataTable.current

let mapCategories f s = { s with Categories = f s.Categories }
let mapStores f s = { s with State.Stores = f s.Stores }
let mapItems f s = { s with Items = f s.Items }

let mapNotSoldItems f s = { s with NotSoldItems = f s.NotSoldItems }

let mapShoppingListSettings f s =
    { s with
          ShoppingListSettings =
              s
              |> shoppingListSettingsRow
              |> DataRow.tryMap f
              |> Result.okOrThrow }

let itemHasInvalidCategory (i: Item) s =
    i.CategoryId
    |> Option.map (fun c ->
        s
        |> categoriesTable
        |> DataTable.tryFindCurrent c
        |> Option.isNone)
    |> Option.defaultValue false

let notSoldHasInvalidItem (ns: NotSoldItem) s =
    s
    |> itemsTable
    |> DataTable.tryFindCurrent ns.ItemId
    |> Option.isNone

let notSoldHasInvalidStore (ns: NotSoldItem) s =
    s
    |> storesTable
    |> DataTable.tryFindCurrent ns.StoreId
    |> Option.isNone

let shoppingListSettingsHasInvalidStoreFilter s =
    s
    |> shoppingListSettings
    |> fun i ->
        i.StoreFilter
        |> Option.map (fun sf ->
            s
            |> storesTable
            |> DataTable.tryFindCurrent sf
            |> Option.isNone)
        |> Option.defaultValue false

let fixItemForeignKeys s =
    s
    |> items
    |> Seq.filter (fun i -> s |> itemHasInvalidCategory i)
    |> Seq.map (fun i -> { i with CategoryId = None })
    |> Seq.fold (fun s i -> s |> mapItems (DataTable.update i)) s

let fixNotSoldForeignKeys s =
    s
    |> notSold
    |> Seq.filter (fun n ->
        let isItemInvalid = s |> notSoldHasInvalidItem n
        let isStoreInvalid = s |> notSoldHasInvalidStore n
        isItemInvalid || isStoreInvalid)
    |> Seq.fold (fun s i -> s |> mapNotSoldItems (DataTable.delete i)) s

let fixShoppingListSettingsForeignKeys s =
    match s |> shoppingListSettingsHasInvalidStoreFilter with
    | false -> s
    | true ->
        s
        |> mapShoppingListSettings ShoppingListSettings.clearStoreFilter

let (insertCategory, updateCategory, upsertCategory) =
    let go f (c: Category) s = s |> mapCategories (f c)
    (go DataTable.insert, go DataTable.update, go DataTable.upsert)

let (insertStore, updateStore, upsertStore) =
    let go f (s: Store) = mapStores (f s)
    (go DataTable.insert, go DataTable.update, go DataTable.upsert)

let (insertItem, updateItem, upsertItem) =
    let go f (i: Item) s =
        match s |> itemHasInvalidCategory i with
        | true -> failwith "The item has an invalid category foreign key."
        | false -> s |> mapItems (f i)

    (go DataTable.insert, go DataTable.update, go DataTable.upsert)

let insertNotSoldItem (n: NotSoldItem) s =
    if s |> notSoldHasInvalidItem n then failwith "The notSoldItem has an invalid item foreign key."
    elif s |> notSoldHasInvalidStore n then failwith "The notSoldItem has an invalid store foreign key."
    else s |> mapNotSoldItems (DataTable.insert n)

let fixForeignKeys s =
    s
    |> fixItemForeignKeys
    |> fixNotSoldForeignKeys
    |> fixShoppingListSettingsForeignKeys

let deleteStore k s = s |> mapStores (DataTable.delete k) |> fixForeignKeys

let deleteItem k s = s |> mapItems (DataTable.delete k) |> fixForeignKeys

let deleteNotSoldItem k s = s |> mapNotSoldItems (DataTable.delete k)

let deleteCategory k s = s |> mapCategories (DataTable.delete k) |> fixForeignKeys

let hasChanges s =
    (s |> itemsTable |> DataTable.hasChanges)
    || (s |> categoriesTable |> DataTable.hasChanges)
    || (s |> storesTable |> DataTable.hasChanges)
    || (s |> notSoldTable |> DataTable.hasChanges)

let importChanges (i: ImportChanges) (s: StateTypes.State) =
    { s with
          Items =
              i.ItemChanges
              |> Seq.fold (fun dt i -> dt |> DataTable.acceptChange i) s.Items
          Categories =
              i.CategoryChanges
              |> Seq.fold (fun dt i -> dt |> DataTable.acceptChange i) s.Categories
          Stores =
              i.StoreChanges
              |> Seq.fold (fun dt i -> dt |> DataTable.acceptChange i) s.Stores
          NotSoldItems =
              i.NotSoldItemChanges
              |> Seq.fold (fun dt i -> dt |> DataTable.acceptChange i) s.NotSoldItems
          LastCosmosTimestamp = i.LatestTimestamp }

let acceptAllChanges s =
    s
    |> mapItems DataTable.acceptChanges
    |> mapCategories DataTable.acceptChanges
    |> mapStores DataTable.acceptChanges
    |> mapNotSoldItems DataTable.acceptChanges
    |> fixForeignKeys

let itemDenormalized (i: Item) state =
    { ItemDenormalized.ItemId = i.ItemId
      ItemName = i.ItemName
      Etag = i.Etag
      Note = i.Note
      Quantity = i.Quantity
      Schedule = i.Schedule
      Category =
          i.CategoryId
          |> Option.map (fun c -> state |> categoriesTable |> DataTable.findCurrent c)
      Availability =
          state
          |> stores
          |> Seq.map (fun s ->
              { ItemAvailability.Store = s
                IsSold =
                    state
                    |> notSoldTable
                    |> DataTable.tryFindCurrent { ItemId = i.ItemId; StoreId = s.StoreId }
                    |> Option.isNone }) }

let createDefault =
    { Categories = DataTable.empty
      Items = DataTable.empty
      Stores = DataTable.empty
      NotSoldItems = DataTable.empty
      ShoppingListSettings = DataRow.unchanged ShoppingListSettings.create
      LastCosmosTimestamp = None
      CategoryEditPage = None
      StoreEditPage = None
      ItemEditPage = None }

let createSampleData () =

    let newCategory n =
        insertCategory
            { Category.CategoryId = CategoryId.create ()
              CategoryName = n |> CategoryName.tryParse |> Result.okOrThrow
              Etag = None }

    let newStore n =
        insertStore
            { Store.StoreId = StoreId.create ()
              StoreName = n |> StoreName.tryParse |> Result.okOrThrow
              Etag = None }

    let findCategory n (s: State) =
        s.Categories
        |> DataTable.current
        |> Seq.find (fun i -> i.CategoryName = (CategoryName.tryParse n |> Result.okOrThrow))

    let findItem n (s: State) =
        s.Items
        |> DataTable.current
        |> Seq.find (fun i -> i.ItemName = (ItemName.tryParse n |> Result.okOrThrow))

    let findStore n (s: State) =
        s.Stores
        |> DataTable.current
        |> Seq.find (fun i -> i.StoreName = (StoreName.tryParse n |> Result.okOrThrow))

    let newItem name cat qty note s =
        s
        |> insertItem
            { Item.ItemId = ItemId.create ()
              ItemName = name |> ItemName.tryParse |> Result.okOrThrow
              Etag = None
              Quantity =
                  qty
                  |> tryParseOptional Quantity.tryParse
                  |> Result.okOrThrow
              Note = note |> tryParseOptional Note.tryParse |> Result.okOrThrow
              Item.Schedule = Schedule.Once
              Item.CategoryId = if cat = "" then None else Some (findCategory cat s).CategoryId }

    let now = System.DateTimeOffset.Now

    let markComplete n s =
        let item =
            s
            |> findItem n
            |> fun i -> { i with Schedule = Schedule.Completed }

        s |> mapItems (DataTable.update item)

    let makeRepeat n freq postpone s =
        let freq = Frequency.create freq |> Result.okOrThrow

        let postpone = postpone |> Option.map (fun d -> now.AddDays(d |> float))

        let repeat = { Repeat.Frequency = freq; PostponedUntil = postpone }

        let item =
            s
            |> findItem n
            |> fun i -> { i with Schedule = Schedule.Repeat repeat }

        s |> mapItems (DataTable.update item)

    let doesNotSellItem store item (s: State) =
        let ns =
            { NotSoldItem.ItemId = (findItem item s).ItemId
              StoreId = (findStore store s).StoreId }

        s |> mapNotSoldItems (DataTable.insert ns)

    createDefault
    |> newCategory "Meat and Seafood"
    |> newCategory "Dairy"
    |> newCategory "Frozen"
    |> newCategory "Produce"
    |> newCategory "Dry"
    |> newItem "Bananas" "Produce" "1 bunch" ""
    |> newItem "Frozen mango chunks" "Frozen" "1 bag" ""
    |> newItem "Apples" "Produce" "6 large" ""
    |> newItem "Chocolate bars" "Dry" "Assorted; many" "Prefer Eco brand"
    |> newItem "Peanut butter" "Dry" "Several jars" "Like Santa Cruz brand"
    |> newItem "Nancy's lowfat yogurt" "Dairy" "1 tub" "Check the date"
    |> newItem "Ice cream" "Frozen" "2 pints" ""
    |> newItem "Dried flax seeds" "Dry" "1 bag" ""
    |> makeRepeat "Bananas" 7<days> None
    |> makeRepeat "Peanut butter" 14<days> (Some 3<days>)
    |> makeRepeat "Apples" 14<days> (Some -3<days>)
    |> markComplete "Ice cream"
    |> newStore "QFC"
    |> newStore "Whole Foods"
    |> newStore "Trader Joe's"
    |> newStore "Costco"
    |> newStore "Walgreens"
    |> doesNotSellItem "QFC" "Dried flax seeds"
    |> doesNotSellItem "Costco" "Chocolate bars"

let handleItemMessage now msg (s: State) =
    match msg with
    | ModifyItem (k, msg) ->
        let item = s.Items |> DataTable.findCurrent k |> Item.update now msg
        s |> updateItem item
    | DeleteItem k -> s |> deleteItem k

let handleCategoryEditPageMessage (msg: CategoryEditPageMessage) (s: State) =
    let form (s: State) =
        s.CategoryEditPage
        |> Option.asResult "There is no category editing form."

    let cancel s = { s with CategoryEditPage = None }

    match msg with
    | BeginEditCategory id ->
        let id = id |> CategoryId.deserialize |> Option.get

        let cat = s |> categoriesTable |> DataTable.findCurrent id

        let form = CategoryEditForm.editExisting cat
        { s with CategoryEditPage = Some form }
    | BeginCreateNewCategory ->
        let form = CategoryEditForm.createNew
        { s with CategoryEditPage = Some form }
    | CategoryEditFormMessage msg ->
        result {
            let! form = s |> form
            let form = form |> CategoryEditForm.handle msg
            return { s with CategoryEditPage = Some form }
        }
        |> Result.okOrThrow
    | SubmitCategoryEditForm ->
        result {
            let! form = s |> form

            let! form =
                form
                |> CategoryEditForm.tryCommit
                |> Result.mapError (sprintf "%A")

            let s =
                match form with
                | CategoryEditForm.FormResult.InsertCategory c ->
                    s
                    |> insertCategory
                        { CategoryName = c
                          CategoryId = CategoryId.create ()
                          Etag = None }
                | CategoryEditForm.FormResult.EditCategory c -> s |> updateCategory c

            return s |> cancel
        }
        |> Result.okOrThrow
    | CancelCategoryEditForm -> s |> cancel
    | DeleteCategory ->
        result {
            let! form = s |> form

            let! id =
                form.CategoryId
                |> Option.asResult "Can only delete an existing category, not one that is being created."

            return s |> deleteCategory id |> cancel
        }
        |> Result.okOrThrow

let handleStoreEditPageMessage (msg: StoreEditPageMessage) (s: State) =
    let form (s: State) =
        s.StoreEditPage
        |> Option.asResult "There is no Store editing form."

    let cancel s = { s with StoreEditPage = None }

    match msg with
    | BeginEditStore id ->
        let id = id |> StoreId.deserialize |> Option.get

        let str = s |> storesTable |> DataTable.findCurrent id

        let form = StoreEditForm.editExisting str
        { s with StoreEditPage = Some form }
    | BeginCreateNewStore ->
        let form = StoreEditForm.createNew
        { s with StoreEditPage = Some form }
    | StoreEditFormMessage msg ->
        result {
            let! form = s |> form
            let form = form |> StoreEditForm.update msg
            return { s with StoreEditPage = Some form }
        }
        |> Result.okOrThrow
    | SubmitStoreEditForm ->
        result {
            let! form = s |> form

            let! form =
                form
                |> StoreEditForm.tryCommit
                |> Result.mapError (sprintf "%A")

            let s =
                match form with
                | StoreEditForm.FormResult.InsertStore c ->
                    s
                    |> insertStore { StoreName = c; StoreId = StoreId.create (); Etag = None }
                | StoreEditForm.FormResult.EditStore c -> s |> updateStore c

            return s |> cancel
        }
        |> Result.okOrThrow
    | CancelStoreEditForm -> s |> cancel
    | DeleteStore ->
        result {
            let! form = s |> form

            let! id =
                form.StoreId
                |> Option.asResult "Can only delete an existing Store, not one that is being created."

            return s |> deleteStore id |> cancel
        }
        |> Result.okOrThrow

// problem when it is deleted; shouldn't happen
let handleShoppingListSettingsMessage (msg: ShoppingListSettings.Message) (s: State) =
    let settings = s |> shoppingListSettings |> ShoppingListSettings.update msg
    s |> mapShoppingListSettings (fun _ -> settings)

let handleItemEditPageMessage (now: DateTimeOffset) (msg: ItemEditPageMessage) (s: State) =
    let form state =
        state.ItemEditPage
        |> Option.asResult "No form is being edited."

    let cancel state = { state with ItemEditPage = None }

    let beginCreateItem name state =
        let categories = state |> categories
        let stores = state |> stores
        let name = name |> Option.defaultValue ""

        let form = ItemForm.createNewItem name stores categories

        { state with ItemEditPage = Some form }

    let processResult (r: ItemForm.ItemFormResult) (s: State) =
        let s =
            match r.InsertCategory with
            | None -> s
            | Some c -> s |> insertCategory c

        let s = s |> upsertItem r.Item

        let nsExpected = r.NotSold

        let nsCurrent =
            s
            |> notSold
            |> Seq.choose (fun i -> if i.ItemId = r.Item.ItemId then Some i.StoreId else None)

        let nsToRemove = nsCurrent |> Seq.except nsExpected
        let nsToAdd = nsExpected |> Seq.except nsCurrent

        let s =
            nsToRemove
            |> Seq.map (fun s -> { StoreId = s; ItemId = r.Item.ItemId })
            |> Seq.fold (fun s i -> s |> deleteNotSoldItem i) s

        let s =
            nsToAdd
            |> Seq.map (fun s -> { StoreId = s; ItemId = r.Item.ItemId })
            |> Seq.fold (fun s i -> s |> insertNotSoldItem i) s

        s

    match msg with
    | BeginEditItem id ->
        let k = ItemId.deserialize id |> Option.get

        let item = s |> itemsTable |> DataTable.findCurrent k

        let item = s |> itemDenormalized item
        let cats = s |> categories
        let form = ItemForm.editItem now cats item
        { s with ItemEditPage = Some form }

    | BeginCreateNewItem -> s |> beginCreateItem None
    | BeginCreateNewItemWithName txt -> s |> beginCreateItem (Some txt)
    | ItemEditFormMessage msg ->
        { s with
              ItemEditPage = s.ItemEditPage |> Option.map (ItemForm.update msg) }
    | SubmitItemEditForm ->

        result {
            let! form = s |> form
            let itemFormResult = form |> ItemForm.asItemFormResult now
            let s = s |> processResult itemFormResult
            let s = s |> cancel
            return s
        }
        |> Result.okOrThrow

    | CancelItemEditForm -> s |> cancel
    | ItemEditPageMessage.DeleteItem ->
        result {
            let! form = s |> form

            let! id =
                form.ItemId
                |> Option.asResult "Can not delete an item without an ID."

            let state = s |> deleteItem id |> cancel

            return state
        }
        |> Result.okOrThrow

let update: Update =
    fun clock msg s ->
        let now = clock ()

        match msg with
        | ItemMessage msg -> s |> handleItemMessage now msg
        | CategoryEditPageMessage msg -> s |> handleCategoryEditPageMessage msg
        | StoreEditPageMessage msg -> s |> handleStoreEditPageMessage msg
        | AcceptAllChanges -> s |> acceptAllChanges
        | Import c -> s |> importChanges c
        | ResetToSampleData -> createSampleData ()
        | ShoppingListSettingsMessage msg -> s |> handleShoppingListSettingsMessage msg
        | ItemEditPageMessage msg -> s |> handleItemEditPageMessage now msg
