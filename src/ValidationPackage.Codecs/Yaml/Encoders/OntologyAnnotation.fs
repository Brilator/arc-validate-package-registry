namespace ValidationPackage.Codecs.Yaml.Encoders

open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal OntologyAnnotation =

    let encode (annotation: ValidationPackage.Model.OntologyAnnotation) =
        Encoding.object [
            "Name", Encoding.string annotation.Name
            "TermSourceREF", Encoding.string annotation.TermSourceREF
            "TermAccessionNumber", Encoding.string annotation.TermAccessionNumber
        ]
