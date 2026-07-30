namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module ValidationPackageJson =

    let encoder = Json.Encoders.ValidationPackage.encode
    let decoder = Json.Decoders.ValidationPackage.decoder

    let encode metadata =
        JsonRuntime.encode encoder metadata

    let decode json =
        JsonRuntime.decode decoder json

    let decodeOrFail json =
        match decode json with
        | Ok metadata -> metadata
        | Error message -> invalidArg "json" message
