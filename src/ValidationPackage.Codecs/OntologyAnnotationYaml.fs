namespace ValidationPackage.Codecs

open YAMLicious
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module OntologyAnnotationYaml =

    let internal encoder (annotation: OntologyAnnotation) =
        YamlValue.object [
            "Name", YamlValue.string annotation.Name
            "TermSourceREF", YamlValue.string annotation.TermSourceREF
            "TermAccessionNumber", YamlValue.string annotation.TermAccessionNumber
        ]

    let decoder =
        Decode.object (fun get ->
            OntologyAnnotation.create(
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
