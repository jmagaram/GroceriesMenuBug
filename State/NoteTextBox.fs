﻿module NoteTextBox

let normalize = trim

let validate t =
    match t |> normalize |> String.length with
    | x when x > 100 -> Some "The note is too long."
    | _ -> None

let create = TextBoxOld.create validate normalize

let update = TextBoxOld.update validate normalize