namespace ValidationPackage.Codecs.Json.Decoders

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal ValidationPackage =

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

            metadata.PreReleaseVersionSuffix <- stringField "preReleaseVersionSuffix"
            metadata.BuildMetadataVersionSuffix <- stringField "buildMetadataVersionSuffix"

            metadata.Publish <-
                get.Optional.Field "publish" Decode.bool
                |> Option.defaultValue false

            metadata.Authors <-
                get.Optional.Field "authors" (Decode.array Author.decoder)
                |> Option.defaultValue Array.empty

            metadata.Tags <-
                get.Optional.Field "tags" (Decode.array OntologyAnnotation.decoder)
                |> Option.defaultValue Array.empty

            metadata.ReleaseNotes <- stringField "releaseNotes"
            metadata.CQCHookEndpoint <- stringField "cqcHookEndpoint"

            metadata.Inputs <-
                get.Optional.Field "Inputs" (Decode.array Cwl.commandInputParameter)
                |> Option.defaultValue Array.empty

            metadata
        )
