namespace ValidationPackage.Codecs

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module ValidationPackageJson =

    let encoder (metadata: ValidationPackageMetadata) =
        Encode.object [
            "name", Encode.string metadata.Name
            "summary", Encode.string metadata.Summary
            "description", Encode.string metadata.Description
            "majorVersion", Encode.int metadata.MajorVersion
            "minorVersion", Encode.int metadata.MinorVersion
            "patchVersion", Encode.int metadata.PatchVersion
            "preReleaseVersionSuffix", Encode.string metadata.PreReleaseVersionSuffix
            "buildMetadataVersionSuffix", Encode.string metadata.BuildMetadataVersionSuffix
            "programmingLanguage", Encode.string metadata.ProgrammingLanguage
            "publish", Encode.bool metadata.Publish
            "authors", metadata.Authors |> Array.map AuthorJson.encoder |> Encode.array
            "tags", metadata.Tags |> Array.map OntologyAnnotationJson.encoder |> Encode.array
            "releaseNotes", Encode.string metadata.ReleaseNotes
            "cqcHookEndpoint", Encode.string metadata.CQCHookEndpoint
            "Inputs",
                metadata.Inputs
                |> Array.map CwlJson.commandInputParameterEncoder
                |> Encode.array
        ]

    let decoder: Decoder<ValidationPackageMetadata> =
        Decode.object (fun get ->
            let stringField name =
                get.Optional.Field name Decode.string
                |> Option.defaultValue ""

            let intField name =
                get.Optional.Field name Decode.int
                |> Option.defaultValue -1

            let metadata =
                ValidationPackageMetadata.create(
                    stringField "name",
                    stringField "summary",
                    stringField "description",
                    intField "majorVersion",
                    intField "minorVersion",
                    intField "patchVersion",
                    stringField "programmingLanguage"
                )

            metadata.PreReleaseVersionSuffix <-
                stringField "preReleaseVersionSuffix"

            metadata.BuildMetadataVersionSuffix <-
                stringField "buildMetadataVersionSuffix"

            metadata.Publish <-
                get.Optional.Field "publish" Decode.bool
                |> Option.defaultValue false

            metadata.Authors <-
                get.Optional.Field
                    "authors"
                    (Decode.array AuthorJson.decoder)
                |> Option.defaultValue Array.empty

            metadata.Tags <-
                get.Optional.Field
                    "tags"
                    (Decode.array OntologyAnnotationJson.decoder)
                |> Option.defaultValue Array.empty

            metadata.ReleaseNotes <- stringField "releaseNotes"
            metadata.CQCHookEndpoint <- stringField "cqcHookEndpoint"

            metadata.Inputs <-
                get.Optional.Field
                    "Inputs"
                    (Decode.array CwlJson.commandInputParameterDecoder)
                |> Option.defaultValue Array.empty

            metadata
        )

    let encode metadata =
        JsonRuntime.encode encoder metadata

    let decode json =
        JsonRuntime.decode decoder json

    let decodeOrFail json =
        match decode json with
        | Ok metadata -> metadata
        | Error message -> invalidArg "json" message
