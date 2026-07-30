namespace AVPR.Staging

open System.IO
open System.Text

[<RequireQualifiedAccess>]
module NormalizedContent =

    let fromString (content: string) =
        content.ReplaceLineEndings("\n")
        |> Encoding.UTF8.GetBytes

    let fromFile path =
        path
        |> File.ReadAllText
        |> fromString
