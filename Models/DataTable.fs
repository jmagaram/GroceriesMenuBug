﻿module Models.DataTable

open ChangeTrackerTypes

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

let hasChanges dt =
    dt
    |> asMap
    |> Map.exists (fun k v -> v |> DataRow.hasChanges)

let private chooseRow f dt =
    dt
    |> asMap
    |> Map.toSeq
    |> Seq.choose (fun (k, v) -> f v |> Option.map (fun v -> (k, v)))
    |> Map.ofSeq
    |> fromMap

let acceptChanges dt = dt |> chooseRow DataRow.acceptChanges

let rejectChanges dt = dt |> chooseRow DataRow.rejectChanges

let isAddedOrModified dt =
    dt
    |> asMap
    |> Map.values
    |> Seq.choose DataRow.isAddedOrModified

let isDeleted dt = dt |> asMap |> Map.values |> Seq.choose DataRow.isDeleted

let deleteIf p dt =
    dt
    |> current
    |> Seq.choose (fun v -> if p v then Some(keyOf v) else None)
    |> Seq.fold (fun dt k -> dt |> delete k) dt

let tryFindCurrent k dt =
    dt
    |> asMap
    |> Map.tryFind k
    |> Option.bind DataRow.currentValue

let findCurrent k dt = tryFindCurrent k dt |> Option.get

// problem with exceptions on delete;
// problem with exceptions on upsert; it will throw but didn't know that from type signature
// really should return results from other functions
let acceptChange c dt =
    match c with
    | Delete k ->
        match dt |> asMap |> Map.tryFind k with
        | None -> dt
        | Some _ -> dt |> asMap |> Map.remove k |> fromMap
    | Upsert v ->
        let key = keyOf v
        let row = DataRow.unchanged v
        dt |> asMap |> Map.add key row |> fromMap
