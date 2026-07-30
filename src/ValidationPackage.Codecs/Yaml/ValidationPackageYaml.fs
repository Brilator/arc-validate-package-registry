namespace ValidationPackage.Codecs

open YAMLicious

[<RequireQualifiedAccess>]
module ValidationPackageYaml =

    let internal encoder = Yaml.Encoders.ValidationPackage.encode
    let decoder = Yaml.Decoders.ValidationPackage.decoder

    let encode metadata =
        metadata
        |> encoder
        |> Yaml.Encoding.write

    let decode yaml =
        try
            yaml
            |> YAMLicious.Reader.read
            |> decoder
            |> Ok
        with error ->
            Error error.Message

    let decodeOrFail yaml =
        match decode yaml with
        | Ok metadata -> metadata
        | Error message -> invalidArg "yaml" message

    let extract language source =
        try
            let metadata =
                source
                |> Frontmatter.extract language
                |> decodeOrFail

            metadata.ProgrammingLanguage <- FrontmatterLanguage.toString language
            Ok metadata
        with error ->
            Error error.Message

    let extractOrFail language source =
        match extract language source with
        | Ok metadata -> metadata
        | Error message -> invalidArg "source" message
