namespace ValidationPackage.Codecs

open YAMLicious
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module ValidationPackageYaml =

    let internal encoder (metadata: ValidationPackageMetadata) =
        YamlValue.object [
            "Name", YamlValue.string metadata.Name
            "Summary", YamlValue.string metadata.Summary
            "Description", YamlValue.string metadata.Description
            "MajorVersion", YamlValue.int metadata.MajorVersion
            "MinorVersion", YamlValue.int metadata.MinorVersion
            "PatchVersion", YamlValue.int metadata.PatchVersion
            "PreReleaseVersionSuffix", YamlValue.string metadata.PreReleaseVersionSuffix
            "BuildMetadataVersionSuffix", YamlValue.string metadata.BuildMetadataVersionSuffix
            "ProgrammingLanguage", YamlValue.string metadata.ProgrammingLanguage
            "Publish", YamlValue.bool metadata.Publish
            "Authors", metadata.Authors |> Array.map AuthorYaml.encoder |> YamlValue.array
            "Tags", metadata.Tags |> Array.map OntologyAnnotationYaml.encoder |> YamlValue.array
            "ReleaseNotes", YamlValue.string metadata.ReleaseNotes
            "CQCHookEndpoint", YamlValue.string metadata.CQCHookEndpoint
            "Inputs", CwlYaml.commandInputParametersEncoder metadata.Inputs
        ]

    let decoder =
        Decode.object (fun get ->
            let stringField name =
                get.Optional.Field name Decode.string
                |> Option.defaultValue ""

            let intField name =
                get.Optional.Field name Decode.int
                |> Option.defaultValue -1

            let metadata =
                ValidationPackageMetadata.create(
                    stringField "Name",
                    stringField "Summary",
                    stringField "Description",
                    intField "MajorVersion",
                    intField "MinorVersion",
                    intField "PatchVersion",
                    stringField "ProgrammingLanguage"
                )

            metadata.PreReleaseVersionSuffix <-
                stringField "PreReleaseVersionSuffix"

            metadata.BuildMetadataVersionSuffix <-
                stringField "BuildMetadataVersionSuffix"

            metadata.Publish <-
                get.Optional.Field "Publish" Decode.bool
                |> Option.defaultValue false

            metadata.Authors <-
                get.Optional.Field "Authors" (Decode.array AuthorYaml.decoder)
                |> Option.defaultValue Array.empty

            metadata.Tags <-
                get.Optional.Field
                    "Tags"
                    (Decode.array OntologyAnnotationYaml.decoder)
                |> Option.defaultValue Array.empty

            metadata.ReleaseNotes <- stringField "ReleaseNotes"
            metadata.CQCHookEndpoint <- stringField "CQCHookEndpoint"

            metadata.Inputs <-
                get.Optional.Field
                    "Inputs"
                    CwlYaml.commandInputParametersDecoder
                |> Option.defaultValue Array.empty

            metadata
        )

    let encode metadata =
        metadata
        |> encoder
        |> YamlValue.write

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

            metadata.ProgrammingLanguage <-
                FrontmatterLanguage.toString language

            Ok metadata
        with error ->
            Error error.Message

    let extractOrFail language source =
        match extract language source with
        | Ok metadata -> metadata
        | Error message -> invalidArg "source" message
