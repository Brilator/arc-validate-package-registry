namespace AVPR.Staging

open System
open System.Security.Cryptography

[<RequireQualifiedAccess>]
module ContentHash =

    let ofBytes (content: byte array) =
        use md5 = MD5.Create()

        content
        |> md5.ComputeHash
        |> Convert.ToHexString

    let ofString content =
        content
        |> NormalizedContent.fromString
        |> ofBytes

    let ofFile path =
        path
        |> NormalizedContent.fromFile
        |> ofBytes
