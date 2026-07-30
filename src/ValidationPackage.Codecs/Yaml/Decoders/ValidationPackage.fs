namespace ValidationPackage.Codecs.Yaml.Decoders

open YAMLicious
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal ValidationPackage =

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

            metadata.PreReleaseVersionSuffix <- stringField "PreReleaseVersionSuffix"
            metadata.BuildMetadataVersionSuffix <- stringField "BuildMetadataVersionSuffix"

            metadata.Publish <-
                get.Optional.Field "Publish" Decode.bool
                |> Option.defaultValue false

            metadata.Authors <-
                get.Optional.Field "Authors" (Decode.array Author.decoder)
                |> Option.defaultValue Array.empty

            metadata.Tags <-
                get.Optional.Field "Tags" (Decode.array OntologyAnnotation.decoder)
                |> Option.defaultValue Array.empty

            metadata.ReleaseNotes <- stringField "ReleaseNotes"
            metadata.CQCHookEndpoint <- stringField "CQCHookEndpoint"

            metadata.Inputs <-
                get.Optional.Field "Inputs" Cwl.commandInputParameters
                |> Option.defaultValue Array.empty

            metadata
        )
