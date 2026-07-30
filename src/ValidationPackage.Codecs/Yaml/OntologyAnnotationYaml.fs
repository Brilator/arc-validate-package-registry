namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module OntologyAnnotationYaml =

    let internal encoder = Yaml.Encoders.OntologyAnnotation.encode
    let decoder = Yaml.Decoders.OntologyAnnotation.decoder
