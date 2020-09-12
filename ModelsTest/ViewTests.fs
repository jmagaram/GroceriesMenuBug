namespace ModelsTest

open System
open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open Models.ValidationTypes

module HighlighterTests =

    open Models
    open Models.ViewTypes
    open Generators

    [<Fact>]
    let ``highlighter - when search an empty string find nothing`` () =
        let searchTerm = "abc" |> SearchTerm.tryParse |> Result.okOrThrow
        let highlighter = searchTerm |> Highlighter.create
        let source = ""
        let actual = source |> highlighter |> FormattedText.spans
        let expected = []
        actual |> should equal expected

    [<Fact>]
    let ``highlighter - when search a whitespace string find exactly that`` () =
        let searchTerm = "abc" |> SearchTerm.tryParse |> Result.okOrThrow
        let highlighter = searchTerm |> Highlighter.create
        let source = "     "
        let actual = source |> highlighter |> FormattedText.spans
        let expected = [ TextSpan.normal source ]
        actual |> should equal expected

    [<Theory>]
    [<InlineData("Once at beginning", "123xyz", "123", "!123,xyz")>]
    [<InlineData("Once at end", "123xyz", "xyz", "123,!xyz")>]
    [<InlineData("At beginning and end", "123xyz123", "123", "!123,xyz,!123")>]
    [<InlineData("Case insensitive", "abcxyz", "XYZ", "abc,!xyz")>]
    [<InlineData("Overlapping matches 1", "aaaaaa", "aa", "!aaaaaa")>]
    [<InlineData("Overlapping matches 2", "ababaccaba", "aba", "!ababa,cc,!aba")>]
    [<InlineData("Overlapping matches 3", "aaabaaaab", "aa", "!aaa,b,!aaaa,b")>]
    [<InlineData("Overlapping matches 4", "xxaabbbaabbbaaxx", "aabbbaa", "xx,!aabbbaabbbaa,xx")>]
    [<InlineData("Overlapping matches 5", "xxabbbbabbbbaxx", "abbbba", "xx,!abbbbabbbba,xx")>]
    [<InlineData("Overlapping matches 6",
                 "baaaaacaaaccaa aaacaaa aac b",
                 "aa",
                 "b,!aaaaa,c,!aaa,cc,!aa, ,!aaa,c,!aaa, ,!aa,c b")>]
    [<InlineData("Overlapping matches 7", "AaA", "aA", "!AaA")>]
    [<InlineData("Overlapping matches 8", "AbaaaAAb aabcb", "aAA", "Ab,!aaaAA,b aabcb")>]
    [<InlineData("Overlapping matches 9", "AAAa", "aaa", "!AAAa")>]
    [<InlineData("Overlapping matches 10", "abaababa", "aba", "!abaababa")>]
    [<InlineData("Overlapping matches 11", "abababaaba", "aba", "!abababaaba")>]
    [<InlineData("Complex case 1", "apple pleasant plum", "pl", "ap,!pl,e ,!pl,easant ,!pl,um")>]
    [<InlineData("Not found at all", "abc", "x", "abc")>]
    [<InlineData("Query is same as entire source", "abc", "abc", "!abc")>]
    [<InlineData("Search for regex characters", "abc^$()[]\/?.+*abc", "^$()[]\/?.+*", "abc,!^$()[]\/?.+*,abc")>]
    let ``highlighter - specific examples`` (comment: string)
                                               (source: string)
                                               (searchTerm: string)
                                               (expected: string)
                                               =
        let parseSpan (s: String) =
            if s.[0] = '!' then (s.Substring(1) |> TextSpan.highlight) else s |> TextSpan.normal

        let parseFormattedText (s: String) =
            s.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
            |> Seq.map parseSpan

        let formatSpan (s: TextSpan) =
            match s.Format with
            | Highlight -> sprintf "!%s" s.Text
            | Normal -> s.Text

        let expected =
            expected
            |> parseFormattedText
            |> Seq.map formatSpan
            |> List.ofSeq

        let highlighter =
            searchTerm
            |> SearchTerm.tryParse
            |> Result.map Highlighter.create
            |> Result.okOrThrow

        let actual =
            source
            |> highlighter
            |> FormattedText.spans
            |> List.map formatSpan

        actual |> should equal expected

    type HighlighterTest = { SearchTerm: SearchTerm; Source: string }

    let highlighterTestGen =
        gen {
            let abcd = Gen.elements [ 'a'; 'b'; 'c'; 'd' ]

            let std = letterOrDigit.Select(fun (LetterOrDigit c) -> c)

            let rgx = regexEscape.Select(fun (RegexEscape c) -> c)

            let punc = punctuation.Select(fun (Punctuation c) -> c)

            let sym = symbol.Select(fun (Symbol c) -> c)
            let space = Gen.constant ' '
            let cr = Gen.constant '\r'
            let lf = Gen.constant '\n'

            let charGens =
                [ (3, abcd)
                  (1, Gen.frequency [ (10, std); (1, space) ])
                  (1, rgx)
                  (1, Gen.oneof [ rgx; punc; sym; space ])
                  (1, Gen.oneof [ std; rgx; punc; sym; space ]) ]
                |> Seq.map (fun (n, g) -> Seq.replicate n g)
                |> Seq.concat
                |> Array.ofSeq

            let! charGenIndex = Gen.choose (0, charGens.Length - 1)
            let termChars = charGens.[charGenIndex]

            let sourceChars =
                Gen.frequency [ (20, termChars)
                                (1, cr)
                                (1, lf) ]

            let minLen = SearchTerm.rules.MinLength |> int
            let maxLen = SearchTerm.rules.MaxLength |> int

            let! searchTermLen =
                Gen.frequency [ (1, Gen.choose (minLen, minLen + 5))
                                (1, Gen.choose (minLen, maxLen)) ]

            let! sourceLen =
                Gen.frequency [ (1, Gen.choose (1, 10))
                                (1, Gen.choose (11, 200)) ]

            let! searchTerm =
                termChars
                |> Gen.listOfLength searchTermLen
                |> Gen.map (fun cs -> cs |> Array.ofList |> String)
                |> Gen.map SearchTerm.tryParse
                |> Gen.filter (fun r -> r |> Result.isOk)
                |> Gen.map (fun r -> r |> Result.okOrThrow)

            let! source =
                sourceChars
                |> Gen.listOfLength sourceLen
                |> Gen.map (fun cs -> cs |> Array.ofList |> String)

            return
                { HighlighterTest.SearchTerm = searchTerm
                  Source = source }
        }

    type Generators =
        static member HighlighterTest() = highlighterTestGen |> Arb.fromGen

    [<Property(MaxTest = 1000, Arbitrary = [| typeof<Generators> |])>]
    let ``concatenated spans equal source`` (p: HighlighterTest) =
        p.Source
        |> Highlighter.create p.SearchTerm
        |> FormattedText.spans
        |> Seq.fold (fun total i -> total + i.Text) ""
        |> fun x -> x = p.Source

    [<Property(MaxTest = 10000, Arbitrary = [| typeof<Generators> |])>]
    let ``when search in regular spans will find nothing`` (p: HighlighterTest) =
        p.Source
        |> Highlighter.create p.SearchTerm
        |> FormattedText.spans
        |> Seq.choose (fun i ->
            match i.Format with
            | TextFormat.Normal -> Some i.Text
            | _ -> None)
        |> Seq.forall (fun t ->
            match t
                  |> Highlighter.create p.SearchTerm
                  |> FormattedText.spans
                  |> Seq.tryExactlyOne with
            | Some r -> r.Format = TextFormat.Normal
            | None -> false)

    [<Property(MaxTest = 10000, Arbitrary = [| typeof<Generators> |])>]
    let ``if highlight is longer than filter, then overlapping or back to back filters in source`` (p: HighlighterTest) =
        let endsWithHighlightedMatch filter text =
            text
            |> Highlighter.create p.SearchTerm
            |> FormattedText.spans
            |> fun ss ->
                match ss with
                | [] -> false
                | [ a ] -> a.Format = TextFormat.Highlight
                | [ _; b ] -> b.Format = TextFormat.Highlight
                | _ -> false

        let filterLength =
            match p.SearchTerm with
            | SearchTerm f -> f.Length

        p.Source
        |> Highlighter.create p.SearchTerm
        |> FormattedText.spans
        |> Seq.choose (fun i ->
            match i.Format with
            | TextFormat.Highlight when i.Text.Length > filterLength -> Some i.Text
            | _ -> None)
        |> Seq.forall (fun t ->
            let excess = t.Length - filterLength

            [ 0 .. excess ]
            |> Seq.forall (fun x ->
                let res =
                    endsWithHighlightedMatch p.SearchTerm (t.Substring(x))

                res))

    [<Property(Arbitrary = [| typeof<Generators> |])>]
    let ``results do not depend on case of filter`` (p: HighlighterTest) =
        let mapFilter mapping f =
            match f with
            | SearchTerm t ->
                mapping t
                |> SearchTerm.tryParse
                |> Result.okOrThrow

        let upperFilter = p.SearchTerm |> mapFilter (fun t -> t.ToUpper())

        let lowerFilter = p.SearchTerm |> mapFilter (fun t -> t.ToLower())

        let withUpperFilter =
            p.Source
            |> Highlighter.create upperFilter
            |> FormattedText.spans
            |> List.ofSeq

        let withLowerFilter =
            p.Source
            |> Highlighter.create lowerFilter
            |> FormattedText.spans
            |> List.ofSeq

        withUpperFilter = withLowerFilter

    [<Property(Arbitrary = [| typeof<Generators> |])>]
    let ``results do not depend on case of source text`` (p: HighlighterTest) =
        let withUpperSource =
            p.Source.ToUpper()
            |> Highlighter.create p.SearchTerm
            |> FormattedText.spans
            |> Seq.map (fun t -> { t with Text = t.Text.ToLower() })
            |> List.ofSeq

        let withLowerSource =
            p.Source.ToLower()
            |> Highlighter.create p.SearchTerm
            |> FormattedText.spans
            |> Seq.map (fun t -> { t with Text = t.Text.ToLower() })
            |> List.ofSeq

        withUpperSource = withLowerSource
