namespace ValidationPackage.Codecs.Json.Decoders

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal Author =

    let decoder: Decoder<ValidationPackage.Model.Author> =
        Decode.object (fun get ->
            ValidationPackage.Model.Author.create(
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
