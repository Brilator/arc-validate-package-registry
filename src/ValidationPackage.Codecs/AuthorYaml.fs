namespace ValidationPackage.Codecs

open YAMLicious
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module AuthorYaml =

    let internal encoder (author: Author) =
        YamlValue.object [
            "FullName", YamlValue.string author.FullName
            "Email", YamlValue.string author.Email
            "Affiliation", YamlValue.string author.Affiliation
            "AffiliationLink", YamlValue.string author.AffiliationLink
        ]

    let decoder =
        Decode.object (fun get ->
            Author.create(
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
