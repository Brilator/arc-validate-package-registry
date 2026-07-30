namespace ValidationPackage.Codecs

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module OntologyAnnotationJson =

    let encoder (annotation: OntologyAnnotation) =
        Encode.object [
            "name", Encode.string annotation.Name
            "termSourceREF", Encode.string annotation.TermSourceREF
            "termAccessionNumber", Encode.string annotation.TermAccessionNumber
        ]

    let decoder: Decoder<OntologyAnnotation> =
        Decode.object (fun get ->
            OntologyAnnotation.create(
                get.Optional.Field "name" Decode.string
                |> Option.defaultValue "",
                TermSourceRef = (
                    get.Optional.Field "termSourceREF" Decode.string
                    |> Option.defaultValue ""
                ),
                TermAccessionNumber = (
                    get.Optional.Field "termAccessionNumber" Decode.string
                    |> Option.defaultValue ""
                )
            )
        )
