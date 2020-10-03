﻿namespace Models

open SynchronizationTypes

module DataRow =

    let added v = Added v

    let unchanged v = Unchanged v

    let modified v v' =
        match v = v' with
        | true -> Unchanged v'
        | false ->
            let vKey = v |> keyOf
            let vKey' = v' |> keyOf

            match vKey = vKey' with
            | false -> failwith "It is not possible to change the key when modifying a data row."
            | true -> Modified {| Original = v; Current = v' |}

    let deleted v = Deleted v

    let delete r =
        match r with
        | Unchanged v -> deleted v |> Some
        | Modified m -> deleted m.Original |> Some
        | Added _ -> None
        | Deleted _ -> failwith "Deleted rows can not be deleted again."

    let update v' r =
        match r with
        | Unchanged v -> modified v v'
        | Modified m -> modified m.Original v'
        | Added _ -> added v'
        | Deleted _ -> failwith "Deleted rows can not be updated."

    let currentValue r =
        match r with
        | Unchanged v -> v |> Some
        | Modified m -> m.Current |> Some
        | Added v -> v |> Some
        | Deleted _ -> None

    let mapCurrent f r =
        match r with
        | Unchanged v -> modified v (f v)
        | Modified m -> modified m.Original (f m.Current)
        | Added v -> added (f v)
        | Deleted _ -> r

    let isAddedOrModified r =
        match r with
        | Added v -> Some v
        | Modified m -> Some m.Current
        | _ -> None

    let isDeleted r =
        match r with
        | Deleted v -> Some v
        | _ -> None

    let hasChanges r =
        match r with
        | Unchanged _ -> false
        | _ -> true

    let acceptChanges r =
        match r with
        | Unchanged _ -> r |> Some
        | Modified m -> m.Current |> unchanged |> Some
        | Added v -> v |> unchanged |> Some
        | Deleted _ -> None

    let rejectChanges r =
        match r with
        | Unchanged _ -> r |> Some
        | Modified m -> m.Original |> unchanged |> Some
        | Added _ -> None
        | Deleted v -> v |> unchanged |> Some

module DataTable =

    let private fromMap dt = DataTable dt

    let asMap dt =
        match dt with
        | DataTable dt -> dt

    let empty<'Key, 'T when 'Key: comparison> = Map.empty<'Key, DataRow<'T>> |> DataTable

    let insert v dt =
        let k = v |> keyOf
        let rowHasKey k dt = dt |> asMap |> Map.containsKey k

        match dt |> rowHasKey k with
        | true -> failwith "A row with that key already exists."
        | false -> dt |> asMap |> Map.add k (DataRow.added v) |> fromMap

    let update v dt =
        let k = v |> keyOf
        let dt = dt |> asMap

        match dt |> Map.tryFind k with
        | None -> failwith "A row with the same key does not exist and thus could not be updated."
        | Some r ->
            let r = r |> DataRow.update v
            dt |> Map.add k r |> fromMap

    let upsert v dt =
        let k = v |> keyOf
        let dt = dt |> asMap

        let r =
            match dt |> Map.tryFind k with
            | None -> DataRow.added v
            | Some r -> r |> DataRow.update v

        dt |> Map.add k r |> fromMap

    let delete k dt =
        let dt = dt |> asMap

        match dt |> Map.tryFind k with
        | None -> failwith "A row with that key does not exist and thus no row could be deleted."
        | Some r ->
            match r |> DataRow.delete with
            | Some r -> dt |> Map.add k r
            | None -> dt |> Map.remove k
            |> fromMap

    let current dt =
        dt
        |> asMap
        |> Seq.choose (fun kv -> kv.Value |> DataRow.currentValue)

    let private chooseRow f dt =
        dt
        |> asMap
        |> Map.toSeq
        |> Seq.choose (fun (k, v) -> f v |> Option.map (fun v -> (k, v)))
        |> Map.ofSeq
        |> fromMap

    let hasChanges dt =
        dt
        |> asMap
        |> Map.exists (fun k v -> v |> DataRow.hasChanges)

    let acceptChanges dt = dt |> chooseRow DataRow.acceptChanges

    let rejectChanges dt = dt |> chooseRow DataRow.rejectChanges

    let deletions dt = 
        let chooseDeleted<'T> (r:DataRow<'T>) =
            match r

    let deleteIf p dt =
        dt
        |> current
        |> Seq.choose (fun v -> if p v then Some (keyOf v) else None)
        |> Seq.fold (fun dt k -> dt |> delete k) dt

    let tryFindCurrent k dt =
        dt
        |> asMap
        |> Map.tryFind k
        |> Option.bind DataRow.currentValue

    let currentContainsKey k dt = dt |> tryFindCurrent k |> Option.isSome

    let findCurrent k dt = tryFindCurrent k dt |> Option.get
