namespace ValidationPackage.Codecs

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module AuthorJson =

    let encoder (author: Author) =
        Encode.object [
            "fullName", Encode.string author.FullName
            "email", Encode.string author.Email
            "affiliation", Encode.string author.Affiliation
            "affiliationLink", Encode.string author.AffiliationLink
        ]

    let decoder: Decoder<Author> =
        Decode.object (fun get ->
            Author.create(
                get.Optional.Field "fullName" Decode.string
                |> Option.defaultValue "",
                Email = (
                    get.Optional.Field "email" Decode.string
                    |> Option.defaultValue ""
                ),
                Affiliation = (
                    get.Optional.Field "affiliation" Decode.string
                    |> Option.defaultValue ""
                ),
                AffiliationLink = (
                    get.Optional.Field "affiliationLink" Decode.string
                    |> Option.defaultValue ""
                )
            )
        )
