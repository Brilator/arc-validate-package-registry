namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module AuthorJson =

    let encoder = Json.Encoders.Author.encode
    let decoder = Json.Decoders.Author.decoder
