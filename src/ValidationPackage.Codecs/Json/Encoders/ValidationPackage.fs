namespace ValidationPackage.Codecs.Json.Encoders

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal ValidationPackage =

    let encode (metadata: ValidationPackageMetadata) =
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
            "authors", metadata.Authors |> Array.map Author.encode |> Encode.array
            "tags", metadata.Tags |> Array.map OntologyAnnotation.encode |> Encode.array
            "releaseNotes", Encode.string metadata.ReleaseNotes
            "cqcHookEndpoint", Encode.string metadata.CQCHookEndpoint
            "Inputs", metadata.Inputs |> Array.map Cwl.commandInputParameter |> Encode.array
        ]
