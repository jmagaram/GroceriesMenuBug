﻿namespace Models

open System
open System.Text.RegularExpressions
open ValidationTypes
open StringValidation
open StateTypes
open System.Collections.Generic

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ItemId =

    let create () = newGuid () |> ItemId

    let serialize i =
        match i with
        | ItemId g -> g.ToString()

    let deserialize s =
        s
        |> String.tryParseWith Guid.TryParse
        |> Option.map ItemId

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module StoreId =

    let create () = newGuid () |> StoreId

    let serialize i =
        match i with
        | StoreId g -> g.ToString()

    let deserialize s =
        s
        |> String.tryParseWith Guid.TryParse
        |> Option.map StoreId

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CategoryId =

    let create () = newGuid () |> CategoryId

    let serialize i =
        match i with
        | CategoryId g -> g.ToString()

    let deserialize s =
        s
        |> String.tryParseWith Guid.TryParse
        |> Option.map CategoryId

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Etag =

    let tag (Etag e) = e

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ItemName =

    let rules = singleLine 3<chars> 50<chars>
    let normalizer = String.trim

    let validator =
        rules |> StringValidation.createValidator

    let tryParse =
        StringValidation.createParser normalizer validator ItemName List.head

    let asText (ItemName s) = s

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Note =

    let rules = multipleLine 3<chars> 200<chars>
    let normalizer = String.trim

    let validator =
        rules |> StringValidation.createValidator

    let tryParse =
        StringValidation.createParser normalizer validator Note List.head

    let tryParseOptional s =
        if s |> String.isNullOrWhiteSpace then Ok None else s |> tryParse |> Result.map Some

    let asText (Note s) = s

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Quantity =

    let rules = singleLine 1<chars> 30<chars>
    let normalizer = String.trim

    let validator =
        rules |> StringValidation.createValidator

    let tryParse =
        StringValidation.createParser normalizer validator Quantity List.head

    let tryParseOptional s =
        if s |> String.isNullOrWhiteSpace then Ok None else s |> tryParse |> Result.map Some

    let asText (Quantity s) = s

    type private KnownUnit = { OneOf: string; ManyOf: string }

    let private knownUnits =
        [ { OneOf = "jar"; ManyOf = "jars" }
          { OneOf = "can"; ManyOf = "cans" }
          { OneOf = "ounce"; ManyOf = "ounces" }
          { OneOf = "pound"; ManyOf = "pounds" }
          { OneOf = "gram"; ManyOf = "grams" }
          { OneOf = "head"; ManyOf = "heads" }
          { OneOf = "bunch"; ManyOf = "bunches" }
          { OneOf = "pack"; ManyOf = "packs" }
          { OneOf = "bag"; ManyOf = "bags" }
          { OneOf = "package"
            ManyOf = "packages" }
          { OneOf = "box"; ManyOf = "boxes" }
          { OneOf = "pint"; ManyOf = "pints" }
          { OneOf = "gallon"; ManyOf = "gallons" }
          { OneOf = "container"
            ManyOf = "containers" } ]

    let private manyOf u =
        knownUnits
        |> Seq.where (fun i -> i.OneOf = u || i.ManyOf = u)
        |> Seq.map (fun i -> i.ManyOf)
        |> Seq.tryHead
        |> Option.defaultValue u

    let private oneOf u =
        knownUnits
        |> Seq.where (fun i -> i.OneOf = u || i.ManyOf = u)
        |> Seq.map (fun i -> i.OneOf)
        |> Seq.tryHead
        |> Option.defaultValue u

    let private grammar =
        new Regex("^\s*(\d+)\s*(.*)", RegexOptions.Compiled)

    type private ParsedQuantity = { Quantity: int; Units: string }

    let private format q =
        match q with
        | { Units = "" } -> sprintf "%i" q.Quantity
        | _ -> sprintf "%i %s" q.Quantity q.Units

    let private parse s =
        let m = grammar.Match(s)

        match m.Success with
        | false -> None
        | true ->
            let qty = System.Int32.Parse(m.Groups.[1].Value)
            let units = m.Groups.[2].Value
            Some { Quantity = qty; Units = units }

    let increase qty =
        match isNullOrWhiteSpace qty with
        | true -> Some "2"
        | false ->
            match parse qty with
            | None -> None
            | Some i ->
                let qty = i.Quantity + 1

                let result =
                    { Quantity = qty
                      Units = i.Units |> manyOf }
                    |> format

                Some result

    let decrease qty =
        match parse qty with
        | None -> None
        | Some i ->
            match i.Quantity with
            | x when x <= 1 -> None
            | _ ->
                let qty = i.Quantity - 1

                let result =
                    { Quantity = qty
                      Units =
                          match qty with
                          | 1 -> i.Units |> oneOf
                          | _ -> i.Units |> manyOf }
                    |> format

                Some result

    let increaseQty qty =
        qty |> asText |> increase |> Option.map Quantity // not good logic; Quantity.create makes a Result

    let decreaseQty qty =
        qty |> asText |> decrease |> Option.map Quantity

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CategoryName =

    let rules = singleLine 3<chars> 30<chars>
    let normalizer = String.trim

    let validator =
        rules |> StringValidation.createValidator

    let tryParse =
        StringValidation.createParser normalizer validator CategoryName List.head

    let asText (CategoryName s) = s

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module StoreName =

    let rules = singleLine 3<chars> 30<chars>
    let normalizer = String.trim

    let validator =
        rules |> StringValidation.createValidator

    let tryParse =
        StringValidation.createParser normalizer validator StoreName List.head

    let asText (StoreName s) = s

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Frequency =

    let rules = { Min = 1<days>; Max = 365<days> }

    let create =
        let normalizer = id
        let validator = RangeValidation.createValidator rules
        let onSuccess = Frequency
        let onFailure = id
        RangeValidation.toResult normalizer validator onSuccess onFailure

    let goodDefault = 7<days> |> create |> Result.okOrThrow

    let common =
        [ 1; 3; 7; 14; 30; 60; 90 ]
        |> List.map (fun i -> i * 1<days> |> create |> Result.okOrThrow)

    let days (Frequency v) = v

    let fromNow (now: DateTimeOffset) f = now.AddDays(f |> days |> float)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Repeat =

    let commonPostponeDays =
        [ 1; 3; 7; 14; 30; 60; 90 ]
        |> List.map (fun i -> i * 1<days>)

    let create frequency postponedUntil =
        { Frequency = frequency
          PostponedUntil = postponedUntil }

    let due (now: DateTimeOffset) r =
        r.PostponedUntil
        |> Option.map (fun future ->
            let duration = future - now

            round (duration.TotalDays) |> int |> (*) 1<days>)

    let dueWithin (now: DateTimeOffset) (d: int<days>) r =
        r
        |> due now
        |> Option.map (fun d' -> d' <= d)
        |> Option.defaultValue true

    let completeOne (now: DateTimeOffset) r =
        { r with
              PostponedUntil = r.Frequency |> Frequency.fromNow now |> Some }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Schedule =

    let due (now: DateTimeOffset) s =
        match s with
        | Once -> now
        | Completed -> DateTimeOffset.MaxValue
        | Repeat r -> r.PostponedUntil |> Option.defaultValue now

    let effectiveDueDateComparer (now: DateTimeOffset): IComparer<Schedule> =
        { new IComparer<Schedule> with
            member this.Compare(x, y) =
                let xDue = x |> due now
                let yDue = y |> due now
                DateTimeOffset.Compare(xDue, yDue) }

    let isPostponed s =
        match s with
        | Repeat { PostponedUntil = Some _ } -> true
        | _ -> false

    let isCompleted s =
        match s with
        | Completed -> true
        | _ -> false

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module NotSoldItem =

    let private separator = '|'

    let serialize (ns: NotSoldItem) =
        let storeId = ns.StoreId |> StoreId.serialize

        let itemId = ns.ItemId |> ItemId.serialize

        sprintf "%s%c%s" storeId separator itemId

    let deserialize (s: string) =
        result {
            if s |> String.IsNullOrEmpty then
                return!
                    "Could not deserialize an empty or null string to a NotSoldItem"
                    |> Error
            else
                let parts = s.Split(separator)

                match parts.Length with
                | 2 ->
                    let! storeId =
                        parts.[0]
                        |> StoreId.deserialize
                        |> Option.map Ok
                        |> Option.defaultValue
                            (sprintf "Could not deserialize the store ID in a NotSoldItem: %s" s
                             |> Error)

                    let! itemId =
                        parts.[1]
                        |> ItemId.deserialize
                        |> Option.map Ok
                        |> Option.defaultValue
                            (sprintf "Could not deserialize the item ID in a NotSoldItem: %s" s
                             |> Error)

                    return
                        { NotSoldItem.StoreId = storeId
                          NotSoldItem.ItemId = itemId }
                | _ ->
                    return!
                        s
                        |> sprintf "Attempting to deserialize a NotSoldItem that does not have exactly two parts: %s"
                        |> Error
        }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Item =

    let markComplete (now: DateTimeOffset) (i: Item) =
        match i.Schedule with
        | Completed -> i
        | Once -> { i with Schedule = Completed }
        | Repeat r ->
            { i with
                  Schedule = r |> Repeat.completeOne now |> Repeat }

    let buyAgain (i: Item) =
        match i.Schedule with
        | Completed -> { i with Schedule = Once }
        | Once -> i
        | Repeat _ -> i

    let isPostponed (i: Item) = i.Schedule |> Schedule.isPostponed

    let isCompleted (i: Item) = i.Schedule |> Schedule.isCompleted

    let removePostpone (i: Item) =
        match i.Schedule with
        | Repeat ({ PostponedUntil = Some _ } as r) ->
            { i with
                  Schedule = Repeat { r with PostponedUntil = None } }
        | _ -> i

    let postpone (now: DateTimeOffset) (d: int<days>) (i: Item) =
        match i.Schedule with
        | Repeat r ->
            let r =
                { r with
                      PostponedUntil = now.AddDays(d |> float) |> Some }

            { i with Schedule = Repeat r }
        | _ -> failwith "A non-repeating item can not be postponed."

module SearchTerm =
    let rules =
        { MinLength = 1<chars>
          MaxLength = 20<chars>
          StringRules.OnlyContains =
              [ Letter
                Mark
                Number
                Punctuation
                Space
                Symbol ] }

    let normalizer = String.trim

    let validator =
        rules |> StringValidation.createValidator

    let tryParse s =
        s
        |> normalizer
        |> fun s ->
            match validator s |> Seq.toList with
            | [] -> Ok s
            | errors -> Error errors
        |> Result.mapError List.head
        |> Result.map SearchTerm

    let rec tryCoerce s =
        if s |> isNullOrWhiteSpace
        then None
        else 
            match s |> normalizer |> tryParse with
            | Error IsRequired -> None
            | Error TooShort -> None
            | Ok t -> Some t
            | Error TooLong -> tryCoerce (s.Substring(0,rules.MaxLength |> int))
            | Error InvalidCharacters -> None // better to strip invalid chars

    let value (SearchTerm s) = s

    let toRegex (SearchTerm s) =
        let isRepeating s =
            let len = s |> String.length

            [ 1 .. len ]
            |> Seq.choose (fun i ->
                let endsWith = s.Substring(len - i)
                let n = len / i

                match (endsWith |> String.replicate n) = s with
                | true -> Some(endsWith, n)
                | false -> None)
            |> Seq.filter (fun (_, i) -> i > 1)
            |> Seq.tryHead

        let edgeMiddleEdge s =
            let len = String.length s
            let maxEdgeLength = (len - 1) / 2

            seq { maxEdgeLength .. (-1) .. 1 }
            |> Seq.choose (fun i ->
                let starts = s.Substring(0, i)
                let ends = s.Substring(len - i)

                if starts = ends then
                    let middle = s.Substring(i, len - i * 2)
                    Some {| Edge = starts; Middle = middle |}
                else
                    None)
            |> Seq.tryHead

        let pattern =
            let s = s.ToLowerInvariant()
            let escape s = Regex.Escape(s)

            match s |> isRepeating with
            | Some (x, n) -> sprintf "(%s){%d,}" (escape x) n
            | None ->
                match s |> edgeMiddleEdge with
                | Some i -> sprintf "((%s)+(%s)*)+" (escape s) (escape (i.Middle + i.Edge))
                | None -> sprintf "(%s)+" (escape s)

        let options =
            RegexOptions.IgnoreCase
            ||| RegexOptions.CultureInvariant

        new Regex(pattern, options)


[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Settings =

    let create =
        { Settings.StoreFilter = None
          PostponedViewHorizon = 7<days>
          HideCompletedItems = true 
          ItemTextFilter = None }

    let setStoreFilter f s = { s with StoreFilter = f }

    let clearStoreFilterIf k s =
        if s.StoreFilter = Some k then s |> setStoreFilter None else s

    let setItemFilter f s = { s with ItemTextFilter = f |> SearchTerm.tryCoerce }

    let clearItemFilter s = { s with ItemTextFilter = None }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module State =

    let categoriesTable (s: State) = s.Categories
    let storesTable (s: State) = s.Stores
    let itemsTable (s: State) = s.Items
    let notSoldItemsTable (s: State) = s.NotSoldItems
    let settingsRow (s: State) = s.Settings

    let categories = categoriesTable >> DataTable.current
    let stores = storesTable >> DataTable.current
    let items = itemsTable >> DataTable.current
    let notSoldItems = notSoldItemsTable >> DataTable.current

    let settings =
        settingsRow
        >> DataRow.currentValue
        >> Option.defaultValue Settings.create

    let mapCategories f s = { s with Categories = f s.Categories }
    let mapStores f s = { s with Stores = f s.Stores }
    let mapItems f s = { s with Items = f s.Items }

    let mapNotSoldItems f s =
        { s with
              NotSoldItems = f s.NotSoldItems }

    let mapSettings f s = { s with Settings = s.Settings |> DataRow.mapCurrent f }

    let (insertCategory, updateCategory, upsertCategory) =
        let go f (c: Category) s = s |> mapCategories (f c)
        (go DataTable.insert, go DataTable.update, go DataTable.upsert)

    let (insertStore, updateStore, upsertStore) =
        let go f (s: Store) = mapStores (f s)
        (go DataTable.insert, go DataTable.update, go DataTable.upsert)

    let (insertItem, updateItem, upsertItem) =
        let go f (i: Item) s =
            let isCategoryReferenceValid =
                i.CategoryId
                |> Option.map (fun c ->
                    s.Categories
                    |> DataTable.tryFindCurrent c
                    |> Option.isSome)
                |> Option.defaultValue true

            match isCategoryReferenceValid with
            | false -> failwith "A category is referenced that does not exist."
            | true -> s |> mapItems (f i)

        (go DataTable.insert, go DataTable.update, go DataTable.upsert)

    let insertNotSoldItem (nsi: NotSoldItem) s =
        let item =
            s.Items |> DataTable.tryFindCurrent nsi.ItemId

        let store =
            s.Stores |> DataTable.tryFindCurrent nsi.StoreId

        match item, store with
        | Some _, Some _ -> s |> mapNotSoldItems (DataTable.insert nsi)
        | None, _ -> failwith "A store is referenced that does not exist."
        | _, None -> failwith "An item is referenced that does not exist."

    let updateSettingsStoreFilter k s =
        let isStoreReferenceValid =
            k
            |> Option.map (fun k ->
                s.Stores
                |> DataTable.tryFindCurrent k
                |> Option.isSome)
            |> Option.defaultValue true

        match isStoreReferenceValid with
        | false -> failwith "A store is referenced that does not exist."
        | true -> mapSettings (Settings.setStoreFilter k) s

    let setPostponedViewHorizon d s =
        s
        |> mapSettings (fun i -> { i with PostponedViewHorizon = d })

    let hideCompletedItems b s =
        s
        |> mapSettings (fun i -> { i with HideCompletedItems = b })

    let deleteStore k s =
        s
        |> mapStores (DataTable.delete k)
        |> mapNotSoldItems (DataTable.deleteIf (fun i -> i.StoreId = k))
        |> mapSettings (Settings.clearStoreFilterIf k)

    let deleteNotSoldItem k s =
        s |> mapNotSoldItems (DataTable.delete k)

    let deleteItem k s = s |> mapItems (DataTable.delete k)

    let deleteCategory k s =
        s
        |> mapCategories (DataTable.delete k)
        |> mapItems (fun dt ->
            dt
            |> DataTable.current
            |> Seq.choose (fun i -> if i.CategoryId = Some k then Some { i with CategoryId = None } else None)
            |> Seq.fold (fun dt i -> dt |> DataTable.update i) s.Items)

    let createDefault =
        { Categories = DataTable.empty
          Items = DataTable.empty
          Stores = DataTable.empty
          NotSoldItems = DataTable.empty
          Settings = DataRow.unchanged Settings.create
          LastCosmosTimestamp = None }

    let setBrokenItemToCategoryLinksToNone (s: State) =
        s
        |> items
        |> Seq.filter (fun i ->
            match i.CategoryId with
            | None -> false
            | Some c ->
                s
                |> categoriesTable
                |> DataTable.tryFindCurrent c
                |> Option.isNone)
        |> Seq.fold (fun s i ->
            s
            |> mapItems (DataTable.upsert { i with CategoryId = None })) s

    let removeBrokenNotSoldItemLinks (s: State) =
        { s with
              NotSoldItems =
                  s
                  |> notSoldItemsTable
                  |> DataTable.deleteIf (fun ns ->
                      let isBrokenStore =
                          s
                          |> itemsTable
                          |> DataTable.tryFindCurrent ns.ItemId
                          |> Option.isNone

                      let isBrokenItem =
                          s
                          |> storesTable
                          |> DataTable.tryFindCurrent ns.StoreId
                          |> Option.isNone

                      isBrokenStore || isBrokenItem) }

    let removeBrokenFilterLinks (s: State) =
        match (s |> settings).StoreFilter with
        | None -> s
        | Some sf ->
            match s |> storesTable |> DataTable.tryFindCurrent sf with
            | None ->
                s
                |> mapSettings (fun s -> { s with StoreFilter = None })
            | Some _ -> s

    let fixBrokenForeignKeys (s: State) =
        s
        |> setBrokenItemToCategoryLinksToNone
        |> removeBrokenNotSoldItemLinks
        |> removeBrokenFilterLinks

    let createSampleData () =

        let newCategory n s =
            s
            |> insertCategory
                { Category.CategoryId = CategoryId.create ()
                  CategoryName = n |> CategoryName.tryParse |> Result.okOrThrow
                  Etag = None }

        let newStore n s =
            s
            |> insertStore
                { Store.StoreId = StoreId.create ()
                  StoreName = n |> StoreName.tryParse |> Result.okOrThrow
                  Etag = None }

        let findCategory n (s: State) =
            let n =
                CategoryName.tryParse n |> Result.okOrThrow

            s.Categories
            |> DataTable.current
            |> Seq.find (fun i -> i.CategoryName = n)

        let findItem n (s: State) =
            let n = ItemName.tryParse n |> Result.okOrThrow

            s.Items
            |> DataTable.current
            |> Seq.find (fun i -> i.ItemName = n)

        let findStore n (s: State) =
            let n = StoreName.tryParse n |> Result.okOrThrow

            s.Stores
            |> DataTable.current
            |> Seq.find (fun i -> i.StoreName = n)

        let newItem name cat qty note s =
            s
            |> insertItem
                { Item.ItemId = ItemId.create ()
                  ItemName = name |> ItemName.tryParse |> Result.okOrThrow
                  Etag = None
                  Quantity =
                      if qty = "" then
                          None
                      else
                          qty
                          |> Quantity.tryParse
                          |> Result.okOrThrow
                          |> Some
                  Note =
                      if note = ""
                      then None
                      else note |> Note.tryParse |> Result.okOrThrow |> Some
                  Item.Schedule = Schedule.Once
                  Item.CategoryId = if cat = "" then None else Some (findCategory cat s).CategoryId }

        let now = System.DateTimeOffset.Now

        let markComplete n (s: State) =
            let item =
                s
                |> findItem n
                |> fun i -> { i with Schedule = Completed }

            s |> mapItems (DataTable.update item)

        let makeRepeat n freq postpone (s: State) =
            let freq =
                Frequency.create freq |> Result.okOrThrow

            let postpone =
                postpone
                |> Option.map (fun d -> now.AddDays(d |> float))

            let repeat = Repeat.create freq postpone

            let item =
                s
                |> findItem n
                |> fun i -> { i with Schedule = Repeat repeat }

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

    let submitStoreForm msg s =
        match msg with
        | StoreFormMessage.InsertStore n ->
            s
            |> insertStore
                { StoreId = StoreId.create ()
                  StoreName = n
                  Etag = None }
        | StoreFormMessage.UpdateStore i -> s |> updateStore i

    let submitCategoryForm msg s =
        match msg with
        | CategoryFormMessage.InsertCategory n ->
            s
            |> insertCategory
                { CategoryId = CategoryId.create ()
                  CategoryName = n
                  Etag = None }
        | CategoryFormMessage.UpdateCategory i -> s |> updateCategory i

    let markItemComplete now id s =
        let item =
            s
            |> itemsTable
            |> DataTable.findCurrent id
            |> Item.markComplete now

        s |> mapItems (DataTable.update item)

    let removePostpone id s =
        let item =
            s
            |> itemsTable
            |> DataTable.findCurrent id
            |> Item.removePostpone

        s |> mapItems (DataTable.update item)

    let postponeItem now id d s =
        let item =
            s
            |> itemsTable
            |> DataTable.findCurrent id
            |> Item.postpone now d

        s |> mapItems (DataTable.update item)

    let buyAgain id s =
        let item =
            s
            |> itemsTable
            |> DataTable.findCurrent id
            |> Item.buyAgain

        s |> mapItems (DataTable.update item)

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
              LastCosmosTimestamp = i.LatestTimestamp
        }
        |> fixBrokenForeignKeys

    let acceptAllChanges s =
        s
        |> mapItems DataTable.acceptChanges
        |> mapCategories DataTable.acceptChanges
        |> mapStores DataTable.acceptChanges
        |> mapNotSoldItems DataTable.acceptChanges

    let rec update msg s =
        let now = DateTimeOffset.Now

        match msg with
        | AcceptAllChanges -> s |> acceptAllChanges
        | SubmitStoreForm msg -> s |> submitStoreForm msg
        | SubmitCategoryForm msg -> s |> submitCategoryForm msg
        | Import c -> s |> importChanges c
        | ItemMessage msg ->
            match msg with
            | MarkComplete i -> s |> markItemComplete now i
            | RemovePostpone i -> s |> removePostpone i
            | Postpone (id, d) -> s |> postponeItem now id d
            | BuyAgain i -> s |> buyAgain i
            | InsertItem i -> s |> insertItem i
            | UpdateItem i -> s |> updateItem i
            | UpsertItem i -> s |> upsertItem i
            | DeleteItem k -> s |> deleteItem k
        | StoreMessage msg ->
            match msg with
            | InsertStore i -> s |> insertStore i
            | DeleteStore k -> s |> deleteStore k
        | CategoryMessage msg ->
            match msg with
            | InsertCategory i -> s |> insertCategory i
            | DeleteCategory k -> s |> deleteCategory k
        | NotSoldItemMessage msg ->
            match msg with
            | InsertNotSoldItem i -> s |> insertNotSoldItem i
            | DeleteNotSoldItem i -> s |> deleteNotSoldItem i
        | SettingsMessage msg ->
            match msg with
            | ClearStoreFilter -> s |> updateSettingsStoreFilter None
            | SetStoreFilterTo id -> s |> updateSettingsStoreFilter (Some id)
            | SetPostponedViewHorizon d -> s |> setPostponedViewHorizon d
            | HideCompletedItems b -> s |> hideCompletedItems b
            | ClearItemFilter -> s |> mapSettings (Settings.clearItemFilter)
            | SetItemFilter f -> s |> mapSettings (Settings.setItemFilter f)
        | Transaction msgs -> msgs |> Seq.fold (fun t i -> t |> update i) s
