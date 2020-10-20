﻿namespace Models

open System
open System.Text.RegularExpressions
open System.Runtime.CompilerServices
open CoreTypes
open StringValidation
open ValidationTypes

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Etag =

    let tag (Etag e) = e

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ItemId =

    let create () = newGuid () |> ItemId

    let serialize i =
        match i with
        | ItemId g -> g |> Guid.serialize

    let deserialize s =
        s |> Guid.tryDeserialize |> Option.map ItemId

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

    let asText (Note s) = s

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Quantity =

    let rules = singleLine 1<chars> 30<chars>
    let normalizer = String.trim

    let validator =
        rules |> StringValidation.createValidator

    let tryParse =
        StringValidation.createParser normalizer validator Quantity List.head

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
module Frequency =

    let rules = { Min = 1<days>; Max = 365<days> }

    let days (Frequency v) = v

    let create =
        let normalizer = id
        let validator = RangeValidation.createValidator rules
        let onSuccess = Frequency
        let onFailure = id
        RangeValidation.toResult normalizer validator onSuccess onFailure

    let frequencyDefault = 7<days> |> create |> Result.okOrThrow

    let commonFrequencyChoices =
        [ 1; 3; 7; 14; 30; 60; 90 ]
        |> List.map (fun i -> i * 1<days> |> create |> Result.okOrThrow)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Schedule =

    let commonPostponeChoices =
        [ 1; 3; 7; 14; 30; 60; 90 ]
        |> List.map (fun i -> i * 1<days>)

    let dueDate (now: DateTimeOffset) s =
        match s with
        | Schedule.Completed -> None
        | Schedule.Once -> Some now
        | Schedule.Repeat r -> r.PostponedUntil |> Option.orElse (Some now)

    let isPostponed s =
        match s with
        | Schedule.Repeat { PostponedUntil = Some _ } -> true
        | _ -> false

    let isCompleted (s: Schedule) =
        match s with
        | Schedule.Completed -> true
        | _ -> false

    let postponedUntil s =
        match s with
        | Schedule.Repeat r -> r.PostponedUntil
        | _ -> None

    let postponedUntilDays (now: DateTimeOffset) s =
        match s with
        | Schedule.Repeat r ->
            r.PostponedUntil
            |> Option.map (fun future ->
                let duration = future - now
                round (duration.TotalDays) |> int |> (*) 1<days>)
        | _ -> None

    let completeNext (now: DateTimeOffset) s =
        match s with
        | Schedule.Completed -> s
        | Schedule.Once -> Schedule.Completed
        | Schedule.Repeat r ->
            { r with
                  PostponedUntil =
                      now.AddDays(r.Frequency |> Frequency.days |> float)
                      |> Some }
            |> Schedule.Repeat

    let activate s =
        match s with
        | Schedule.Completed -> Schedule.Once
        | Schedule.Once -> s
        | Schedule.Repeat _ -> s

    let withoutPostpone s =
        match s with
        | Schedule.Repeat ({ PostponedUntil = Some _ } as r) ->
            { r with PostponedUntil = None }
            |> Schedule.Repeat
        | _ -> s

    let tryPostpone (now: DateTimeOffset) (d: int<days>) s =
        match s with
        | Schedule.Repeat r ->
            { r with
                  PostponedUntil = now.AddDays(d |> float) |> Some }
            |> Schedule.Repeat
            |> Ok
        | _ -> Error "Only repeating items can be postponed."

    [<Extension>]
    type ScheduleExtensions =
        [<Extension>]
        static member IsPostponed(me: Schedule) = me |> isPostponed

        [<Extension>]
        static member DueDate(me: Schedule, now: DateTimeOffset) = me |> dueDate now

        [<Extension>]
        static member PostponedUntilDays(me: Schedule, now: DateTimeOffset) = me |> postponedUntilDays now

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Item =

    let mapSchedule f i =
        { i with
              Item.Schedule = i.Schedule |> f }

    let markComplete (now: DateTimeOffset) i =
        i |> mapSchedule (Schedule.completeNext now)

    let buyAgain i = i |> mapSchedule Schedule.activate

    let removePostpone i =
        i |> mapSchedule Schedule.withoutPostpone

    let postpone now days i =
        i
        |> mapSchedule (Schedule.tryPostpone now days >> Result.okOrThrow)

    type Message =
        | MarkComplete
        | BuyAgain
        | RemovePostpone
        | Postpone of int<days>

    let update now msg i =
        match msg with
        | MarkComplete -> i |> markComplete now
        | BuyAgain -> i |> buyAgain
        | RemovePostpone -> i |> removePostpone
        | Postpone d -> i |> postpone now d

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CategoryId =

    let create () = newGuid () |> CategoryId

    let serialize i =
        match i with
        | CategoryId g -> g |> Guid.serialize

    let deserialize s =
        s |> Guid.tryDeserialize |> Option.map CategoryId

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
module StoreId =

    let create () = newGuid () |> StoreId

    let serialize i =
        match i with
        | StoreId g -> g |> Guid.serialize

    let deserialize s =
        s |> Guid.tryDeserialize |> Option.map StoreId

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
module NotSoldItem =

    let private separator = '|'

    let serialize (ns: NotSoldItem) =
        let storeId = ns.StoreId |> StoreId.serialize

        let itemId = ns.ItemId |> ItemId.serialize

        sprintf "%s%c%s" storeId separator itemId

    let deserialize (s: string) =
        result {
            if s |> String.isNullOrWhiteSpace then
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
module TextBox =

    let create s = { ValueTyping = s; ValueCommitted = s }

    let typeText s t = { t with ValueTyping = s }

    let loseFocus normalize t =
        { t with
              ValueCommitted = normalize t.ValueTyping }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
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
        if s |> isNullOrWhiteSpace then
            None
        else
            match s |> normalizer |> tryParse with
            | Error IsRequired -> None
            | Error TooShort -> None
            | Ok t -> Some t
            | Error TooLong -> tryCoerce (s.Substring(0, rules.MaxLength |> int))
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
module ShoppingListSettings =

    let create =
        { ShoppingListSettings.StoreFilter = None
          PostponedViewHorizon = 7<days>
          HideCompletedItems = true
          ItemTextFilter = None }

    let setStoreFilter k s = { s with StoreFilter = Some k }

    let clearStoreFilter s = { s with StoreFilter = None }

    let clearStoreFilterIf k s =
        if s.StoreFilter = Some k then s |> clearStoreFilter else s

    let setItemFilter txt s =
        { s with
              ItemTextFilter = txt |> SearchTerm.tryCoerce }

    let clearItemFilter s = { s with ItemTextFilter = None }

    let hideCompletedItems b s = { s with HideCompletedItems = b }

    let setPostponedViewHorizon d s =
        let d = d |> min 365<days> |> max -365<days>
        { s with PostponedViewHorizon = d }

    type Message =
        | ClearStoreFilter
        | SetStoreFilterTo of StoreId
        | SetPostponedViewHorizon of int<days>
        | HideCompletedItems of bool
        | SetItemFilter of string
        | ClearItemFilter

    let update (msg: Message) s =
        match msg with
        | ClearStoreFilter -> s |> clearStoreFilter
        | SetStoreFilterTo k -> s |> setStoreFilter k
        | SetPostponedViewHorizon d -> s |> setPostponedViewHorizon d
        | HideCompletedItems b -> s |> hideCompletedItems b
        | SetItemFilter txt -> s |> setItemFilter txt
        | ClearItemFilter -> s |> clearItemFilter

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CategoryEditForm =

    type FormMode =
        | CreateNewCategoryMode
        | EditExistingCategoryMode

    type FormResult =
        | InsertCategory of CategoryName
        | EditCategory of Category

    type Message = CategoryNameMessage of TextBoxMessage

    let createNew =
        { CategoryId = None
          CategoryName = TextBox.create ""
          Etag = None }

    let editExisting (c: Category) =
        { CategoryId = Some c.CategoryId
          CategoryName =
              c.CategoryName
              |> CategoryName.asText
              |> TextBox.create
          Etag = c.Etag }

    let typeCategoryName s f =
        { f with
              CategoryEditForm.CategoryName = f.CategoryName |> TextBox.typeText s }

    let blurCategoryName f =
        { f with
              CategoryEditForm.CategoryName =
                  f.CategoryName
                  |> (TextBox.loseFocus CategoryName.normalizer) }

    let validateCategoryName f =
        f.CategoryName.ValueTyping
        |> CategoryName.tryParse

    let hasErrors f =
        f |> validateCategoryName |> Result.isError

    let mode f =
        match f.CategoryId with
        | None -> CreateNewCategoryMode
        | Some _ -> EditExistingCategoryMode

    let tryCommit f =
        result {
            let! name = f |> validateCategoryName

            return
                match f.CategoryId with
                | None -> FormResult.InsertCategory name
                | Some id ->
                    FormResult.EditCategory
                        { CategoryId = id
                          Etag = f.Etag
                          CategoryName = name }
        }

    let handle msg f =
        match msg with
        | CategoryNameMessage txt ->
            match txt with
            | TextBoxMessage.LoseFocus -> f |> blurCategoryName
            | TextBoxMessage.TypeText s -> f |> typeCategoryName s

    [<Extension>]
    type CategoryEditFormExtensions =
        [<Extension>]
        static member HasErrors(me: CategoryEditForm) = me |> hasErrors

        [<Extension>]
        static member CategoryNameErrors(me: CategoryEditForm) =
            me |> validateCategoryName |> Result.error

        [<Extension>]
        static member Mode(me: CategoryEditForm) = me |> mode

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module StoreEditForm =

    type FormResult =
        | InsertStore of StoreName
        | EditStore of Store

    type FormMode =
        | CreateNewStoreMode
        | EditExistingStoreMode

    type Message = StoreNameMessage of TextBoxMessage

    let createNew =
        { StoreId = None
          StoreName = TextBox.create ""
          Etag = None }

    let editExisting (c: Store) =
        { StoreId = Some c.StoreId
          StoreName = c.StoreName |> StoreName.asText |> TextBox.create
          Etag = c.Etag }

    let typeStoreName s f =
        { f with
              StoreEditForm.StoreName = f.StoreName |> TextBox.typeText s }

    let blurStoreName f =
        { f with
              StoreEditForm.StoreName =
                  f.StoreName
                  |> (TextBox.loseFocus StoreName.normalizer) }

    let validateStoreName f =
        f.StoreName.ValueTyping |> StoreName.tryParse

    let hasErrors f = f |> validateStoreName |> Result.isError

    let mode f =
        match f.StoreId with
        | None -> CreateNewStoreMode
        | Some _ -> EditExistingStoreMode

    let tryCommit f =
        result {
            let! name = f |> validateStoreName

            return
                match f.StoreId with
                | None -> FormResult.InsertStore name
                | Some id ->
                    FormResult.EditStore
                        { StoreId = id
                          Etag = f.Etag
                          StoreName = name }
        }

    let update msg f =
        match msg with
        | StoreNameMessage txt ->
            match txt with
            | TextBoxMessage.LoseFocus -> f |> blurStoreName
            | TextBoxMessage.TypeText s -> f |> typeStoreName s

    [<Extension>]
    type StoreEditFormExtensions =
        [<Extension>]
        static member HasErrors(me: StoreEditForm) = me |> hasErrors

        [<Extension>]
        static member StoreNameErrors(me: StoreEditForm) = me |> validateStoreName |> Result.error

        [<Extension>]
        static member Mode(me: StoreEditForm) = me |> mode

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ItemForm =

    let itemNameValidation (f: ItemForm) =
        f.ItemName.ValueTyping |> ItemName.tryParse

    let itemNameChange s (f: ItemForm) =
        { f with
              ItemName = f.ItemName |> TextBox.typeText s }

    let itemNameBlur f =
        { f with
              ItemForm.ItemName =
                  f.ItemName
                  |> TextBox.loseFocus ItemName.normalizer }

    let quantityValidation (f: ItemForm) =
        f.Quantity.ValueTyping
        |> String.tryParseOptional Quantity.tryParse

    let quantityChange s (f: ItemForm) =
        { f with
              Quantity = f.Quantity |> TextBox.typeText s }

    let quantityBlur f =
        { f with
              ItemForm.Quantity =
                  f.Quantity
                  |> TextBox.loseFocus Quantity.normalizer }

    let noteChange s (f: ItemForm) =
        { f with
              Note = f.Note |> TextBox.typeText s }

    let noteValidation (f: ItemForm) =
        f.Note.ValueTyping
        |> String.tryParseOptional Note.tryParse

    let noteBlur f =
        { f with
              ItemForm.Note = f.Note |> TextBox.loseFocus Note.normalizer }

    let categoryModeChooseExisting f = { f with CategoryMode = ChooseExisting }
    let categoryModeCreateNew f = { f with CategoryMode = CreateNew }
    let chooseCategoryUncategorized f = { f with CategoryChoice = None }

    let categoryModeIsCreateNew f =
        match f.CategoryMode with
        | CreateNew -> true
        | _ -> false

    let chooseCategory i f =
        { f with
              CategoryChoice =
                  f.CategoryChoiceList
                  |> List.find (fun j -> j.CategoryId = CategoryId i)
                  |> Some }

    let categoryNameValidation (f: ItemForm) =
        f.NewCategoryName.ValueTyping
        |> CategoryName.tryParse

    let categoryNameChange s (f: ItemForm) =
        { f with
              NewCategoryName = f.NewCategoryName |> TextBox.typeText s }

    let categoryNameBlur (f: ItemForm) =
        let normalized =
            f.NewCategoryName
            |> TextBox.loseFocus CategoryName.normalizer

        let exists =
            f.CategoryChoiceList
            |> Seq.tryFind (fun i ->
                String.Equals
                    (i.CategoryName |> CategoryName.asText,
                     normalized.ValueCommitted,
                     StringComparison.InvariantCultureIgnoreCase))

        match exists with
        | None -> { f with NewCategoryName = normalized }
        | Some c ->
            { f with
                  CategoryMode = ChooseExisting
                  CategoryChoice = Some c
                  NewCategoryName = TextBox.create "" }

    let scheduleOnce f = { f with ScheduleKind = Once }
    let scheduleCompleted f = { f with ScheduleKind = Completed }
    let scheduleRepeat f = { f with ScheduleKind = Repeat }

    let frequencyCoerceIntoBounds d =
        Frequency.rules
        |> RangeValidation.forceIntoBounds d

    let frequencySet v f =
        { f with
              ItemForm.Frequency =
                  v
                  |> frequencyCoerceIntoBounds
                  |> Frequency.create
                  |> Result.okOrThrow }

    let frequencyChoices (f: ItemForm) =
        f.Frequency :: Frequency.commonFrequencyChoices
        |> Seq.distinct
        |> Seq.sort
        |> List.ofSeq

    let frequencyAsText (d: Frequency) =
        let d = d |> Frequency.days |> int

        let monthsExactly =
            d
            |> divRem 30
            |> Option.filter (fun i -> i.Quotient >= 1 && i.Remainder = 0)
            |> Option.map (fun i -> if i.Quotient = 1 then "Monthly" else sprintf "Every %i months" i.Quotient)

        let weeksExactly =
            d
            |> divRem 7
            |> Option.filter (fun i -> i.Quotient >= 1 && i.Remainder = 0)
            |> Option.map (fun i -> if i.Quotient = 1 then "Weekly" else sprintf "Every %i weeks" i.Quotient)

        monthsExactly
        |> Option.orElse weeksExactly
        |> Option.defaultWith (fun () -> if d = 1 then "Daily" else sprintf "Every %i days" d)

    let postponeSet v f = { f with Postpone = Some v }

    let postponeUntilFrequency f =
        { f with
              Postpone = f.Frequency |> Frequency.days |> Some }

    let postponeClear f = { f with Postpone = None }

    let purchased f =
        match f.ScheduleKind with
        | Once -> f |> scheduleCompleted
        | Repeat -> f |> postponeUntilFrequency
        | Completed -> f

    let postponeDurationAsText (d: int<days>) =
        let d = d |> int

        let monthsExactly =
            if d / 30 > 0 && d % 30 = 0 then Some(d / 30) else None

        let weeksExactly =
            if d / 7 > 0 && d % 7 = 0 then Some(d / 7) else None

        match monthsExactly with
        | Some m -> if m = 1 then "1 month" else sprintf "%i months" m
        | None ->
            match weeksExactly with
            | Some w -> if w = 1 then "1 week" else sprintf "%i weeks" w
            | None -> if d = 1 then "1 day" else sprintf "%i days" d

    let postponeChoices (f: ItemForm) =
        f.Postpone
        :: (Schedule.commonPostponeChoices |> List.map Some)
        |> Seq.choose id
        |> Seq.map frequencyCoerceIntoBounds
        |> Seq.distinct
        |> Seq.sort
        |> List.ofSeq

    let storesSetAvailability id isSold f =
        { f with
              ItemForm.Stores =
                  f.Stores
                  |> List.map (fun a -> if a.Store.StoreId = id then { a with IsSold = isSold } else a) }

    let canDelete (f: ItemForm) = f.ItemId.IsSome

    let createNewItem itemName stores cats =
        { ItemId = None
          ItemName = TextBox.create itemName
          Etag = None
          Quantity = TextBox.create ""
          Note = TextBox.create ""
          ScheduleKind = ScheduleKind.Once
          Frequency = Frequency.frequencyDefault
          Postpone = None
          CategoryMode = CategoryMode.ChooseExisting
          NewCategoryName = TextBox.create ""
          CategoryChoice = None
          CategoryChoiceList =
              cats
              |> Seq.sortBy (fun (i: Category) -> i.CategoryName)
              |> List.ofSeq
          Stores =
              stores
              |> Seq.map (fun i ->
                  { ItemAvailability.Store = i
                    ItemAvailability.IsSold = true })
              |> Seq.sortBy (fun i -> i.Store.StoreName)
              |> List.ofSeq }

    let editItem (now:DateTimeOffset) cats (i: ItemDenormalized) =
        { ItemId = Some i.ItemId
          ItemName = i.ItemName |> ItemName.asText |> TextBox.create
          Etag = i.Etag
          Quantity =
              i.Quantity
              |> Option.map Quantity.asText
              |> Option.defaultValue ""
              |> TextBox.create
          Note =
              i.Note
              |> Option.map Note.asText
              |> Option.defaultValue ""
              |> TextBox.create
          ScheduleKind =
              match i.Schedule with
              | Schedule.Completed -> Completed
              | Schedule.Once -> Once
              | Schedule.Repeat _ -> Repeat
          Frequency =
              match i.Schedule with
              | Schedule.Completed -> Frequency.frequencyDefault
              | Schedule.Once -> Frequency.frequencyDefault
              | Schedule.Repeat r -> r.Frequency
          Postpone =
              i.Schedule
              |> Schedule.postponedUntilDays now
          CategoryMode = CategoryMode.ChooseExisting
          NewCategoryName = "" |> TextBox.create
          CategoryChoice = i.Category
          CategoryChoiceList =
              cats
              |> Seq.sortBy (fun (i: Category) -> i.CategoryName)
              |> List.ofSeq
          Stores =
              i.Availability
              |> Seq.sortBy (fun i -> i.Store.StoreName)
              |> List.ofSeq }

    let hasErrors f =
        (f |> itemNameValidation |> Result.isError)
        || (f |> quantityValidation |> Result.isError)
        || (f |> noteValidation |> Result.isError)
        || ((f |> categoryModeIsCreateNew)
            && (f |> categoryNameValidation |> Result.isError))

    type ItemFormResult =
        { Item: Item
          InsertCategory: Category option
          NotSold: StoreId list }

    let asItemFormResult (now: DateTimeOffset) (f: ItemForm) =
        let insertCategory =
            match f.CategoryMode with
            | ChooseExisting -> None
            | CreateNew ->
                f
                |> categoryNameValidation
                |> Result.okOrThrow
                |> fun c ->
                    Some
                        { Category.CategoryName = c
                          Category.CategoryId = CategoryId.create ()
                          Category.Etag = None }

        let item =
            { Item.ItemId =
                  f.ItemId
                  |> Option.defaultWith (fun () -> ItemId.create ())
              Item.ItemName = f |> itemNameValidation |> Result.okOrThrow
              Item.Etag = f.Etag
              Item.CategoryId =
                  match f.CategoryMode with
                  | ChooseExisting -> f.CategoryChoice
                  | CreateNew -> insertCategory
                  |> Option.map (fun i -> i.CategoryId)
              Item.Quantity = f |> quantityValidation |> Result.okOrThrow
              Item.Note = f |> noteValidation |> Result.okOrThrow
              Item.Schedule =
                  match f.ScheduleKind with
                  | Completed -> Schedule.Completed
                  | Once -> Schedule.Once
                  | Repeat ->
                      { Repeat.Frequency = f.Frequency
                        Repeat.PostponedUntil =
                            f.Postpone
                            |> Option.map (fun d -> now.AddDays(d |> float)) }
                      |> Schedule.Repeat }

        let notSold =
            f.Stores
            |> Seq.choose (fun i -> if i.IsSold = false then Some i.Store.StoreId else None)
            |> List.ofSeq

        { Item = item
          InsertCategory = insertCategory
          NotSold = notSold }

    type Message =
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
        | Transaction of Message seq

    let rec update msg (f: ItemForm) =
        match msg with
        | ItemNameSet s -> f |> itemNameChange s
        | ItemNameBlur -> f |> itemNameBlur
        | QuantitySet s -> f |> quantityChange s
        | QuantityBlur -> f |> quantityBlur
        | NoteSet s -> f |> noteChange s
        | NoteBlur -> f |> noteBlur
        | ScheduleOnce -> f |> scheduleOnce
        | ScheduleCompleted -> f |> scheduleCompleted
        | ScheduleRepeat -> f |> scheduleRepeat
        | FrequencySet v -> f |> frequencySet v
        | PostponeSet d -> f |> postponeSet d
        | PostponeClear -> f |> postponeClear
        | CategoryModeChooseExisting -> f |> categoryModeChooseExisting
        | CategoryModeCreateNew -> f |> categoryModeCreateNew
        | ChooseCategoryUncategorized -> f |> chooseCategoryUncategorized
        | ChooseCategory g -> f |> chooseCategory g
        | NewCategoryNameSet s -> f |> categoryNameChange s
        | NewCategoryNameBlur -> f |> categoryNameBlur
        | StoresSetAvailability (id: StoreId, isSold: bool) -> f |> storesSetAvailability id isSold
        | Purchased -> f |> purchased
        | Message.Transaction msgs -> msgs |> Seq.fold (fun f m -> update m f) f

    [<Extension>]
    type ItemFormExtensions =
        [<Extension>]
        static member ItemNameValidation(me: ItemForm) = me |> itemNameValidation

        [<Extension>]
        static member NoteValidation(me: ItemForm) = me |> noteValidation

        [<Extension>]
        static member QuantityValidation(me: ItemForm) = me |> quantityValidation

        [<Extension>]
        static member FrequencyChoices(me: ItemForm) = me |> frequencyChoices

        [<Extension>]
        static member PostponeChoices(me: ItemForm) = me |> postponeChoices

        [<Extension>]
        static member CategoryNameValidation(me: ItemForm) = me |> categoryNameValidation

        [<Extension>]
        static member HasErrors(me: ItemForm) = me |> hasErrors

        [<Extension>]
        static member CanDelete(me: ItemForm) = me |> canDelete