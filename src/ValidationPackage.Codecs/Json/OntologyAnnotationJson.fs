namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module OntologyAnnotationJson =

    let encoder = Json.Encoders.OntologyAnnotation.encode
    let decoder = Json.Decoders.OntologyAnnotation.decoder
