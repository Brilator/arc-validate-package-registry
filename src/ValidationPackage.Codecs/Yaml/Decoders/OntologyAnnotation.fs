namespace ValidationPackage.Codecs.Yaml.Decoders

open YAMLicious
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal OntologyAnnotation =

    let decoder =
        Decode.object (fun get ->
            ValidationPackage.Model.OntologyAnnotation.create(
                get.Optional.Field "Name" Decode.string
                |> Option.defaultValue "",
                TermSourceRef = (
                    get.Optional.Field "TermSourceREF" Decode.string
                    |> Option.defaultValue ""
                ),
                TermAccessionNumber = (
                    get.Optional.Field "TermAccessionNumber" Decode.string
                    |> Option.defaultValue ""
                )
            )
        )
