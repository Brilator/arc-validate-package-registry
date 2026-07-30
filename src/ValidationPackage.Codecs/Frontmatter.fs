namespace ValidationPackage.Codecs

open System

[<RequireQualifiedAccess>]
module Frontmatter =

    [<Literal>]
    let FSharpCommentStart = "(*\n---"

    [<Literal>]
    let FSharpCommentEnd = "---\n*)"

    [<Literal>]
    let FSharpBindingStart = "let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---"

    [<Literal>]
    let FSharpBindingEnd = "---\n*)\"\"\""

    [<Literal>]
    let PythonCommentStart = "\"\"\"\n---"

    [<Literal>]
    let PythonCommentEnd = "---\n\"\"\""

    [<Literal>]
    let PythonBindingStart = "PACKAGE_METADATA = \"\"\"\n---"

    [<Literal>]
    let PythonBindingEnd = "---\n\"\"\""

    let private tryExtractBetween
        (startMarker: string)
        (endMarker: string)
        (source: string)
        =
        if source.StartsWith(startMarker, StringComparison.Ordinal) then
            let endIndex = source.IndexOf(endMarker, StringComparison.Ordinal)

            if endIndex >= startMarker.Length then
                Some(source.Substring(startMarker.Length, endIndex - startMarker.Length))
            else
                None
        else
            None

    let tryExtract language (source: string) =
        if isNull source then
            nullArg "source"

        let normalized = source.Replace("\r\n", "\n").Replace("\r", "\n")

        match language with
        | FrontmatterLanguage.FSharp ->
            tryExtractBetween FSharpCommentStart FSharpCommentEnd normalized
            |> Option.orElseWith (fun () ->
                tryExtractBetween FSharpBindingStart FSharpBindingEnd normalized
            )
        | FrontmatterLanguage.Python ->
            tryExtractBetween PythonCommentStart PythonCommentEnd normalized
            |> Option.orElseWith (fun () ->
                tryExtractBetween PythonBindingStart PythonBindingEnd normalized
            )

    let extract language source =
        match tryExtract language source with
        | Some frontmatter -> frontmatter
        | None ->
            let languageName = FrontmatterLanguage.toString language
            invalidArg "source" $"input has no correctly formatted {languageName} frontmatter"
