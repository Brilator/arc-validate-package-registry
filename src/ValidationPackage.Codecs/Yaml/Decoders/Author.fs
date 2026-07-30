namespace ValidationPackage.Codecs.Yaml.Decoders

open YAMLicious
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal Author =

    let decoder =
        Decode.object (fun get ->
            ValidationPackage.Model.Author.create(
                get.Optional.Field "FullName" Decode.string
                |> Option.defaultValue "",
                Email = (
                    get.Optional.Field "Email" Decode.string
                    |> Option.defaultValue ""
                ),
                Affiliation = (
                    get.Optional.Field "Affiliation" Decode.string
                    |> Option.defaultValue ""
                ),
                AffiliationLink = (
                    get.Optional.Field "AffiliationLink" Decode.string
                    |> Option.defaultValue ""
                )
            )
        )
