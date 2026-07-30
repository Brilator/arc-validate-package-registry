namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module AuthorYaml =

    let internal encoder = Yaml.Encoders.Author.encode
    let decoder = Yaml.Decoders.Author.decoder
