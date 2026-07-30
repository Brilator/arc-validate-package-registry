namespace ValidationPackage.Codecs.Json.Decoders

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal OntologyAnnotation =

    let decoder: Decoder<ValidationPackage.Model.OntologyAnnotation> =
        Decode.object (fun get ->
            ValidationPackage.Model.OntologyAnnotation.create(
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
