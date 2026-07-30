namespace ValidationPackage.Codecs.Json.Encoders

open Thoth.Json.Core

[<RequireQualifiedAccess>]
module internal OntologyAnnotation =

    let encode (annotation: ValidationPackage.Model.OntologyAnnotation) =
        Encode.object [
            "name", Encode.string annotation.Name
            "termSourceREF", Encode.string annotation.TermSourceREF
            "termAccessionNumber", Encode.string annotation.TermAccessionNumber
        ]
