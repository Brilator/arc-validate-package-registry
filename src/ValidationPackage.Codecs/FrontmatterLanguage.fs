namespace ValidationPackage.Codecs

open System

type FrontmatterLanguage =
    | FSharp
    | Python

[<RequireQualifiedAccess>]
module FrontmatterLanguage =

    let fromString (value: string) =
        if isNull value then
            nullArg "value"

        match value.ToLowerInvariant() with
        | "fsharp"
        | "fs"
        | "f#" -> FrontmatterLanguage.FSharp
        | "python"
        | "py" -> FrontmatterLanguage.Python
        | _ -> invalidArg "value" $"unsupported frontmatter language: {value}"

    let toString language =
        match language with
        | FrontmatterLanguage.FSharp -> "FSharp"
        | FrontmatterLanguage.Python -> "Python"
