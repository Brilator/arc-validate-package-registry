namespace AVPR.Staging

open System
open System.IO
open ValidationPackage.Codecs
open ValidationPackage.Model

type StagedValidationPackage =
    {
        RepoPath: string
        FileName: string
        LastUpdated: DateTimeOffset
        ContentHash: string
        Metadata: ValidationPackageMetadata
    }

[<RequireQualifiedAccess>]
module StagedValidationPackage =

    let create repoPath fileName lastUpdated contentHash metadata =
        {
            RepoPath = repoPath
            FileName = fileName
            LastUpdated = lastUpdated
            ContentHash = contentHash
            Metadata = metadata
        }

    let private languageFromPath (path: string) =
        match Path.GetExtension(path).ToLowerInvariant() with
        | ".fsx" -> FrontmatterLanguage.FSharp
        | ".py" -> FrontmatterLanguage.Python
        | extension ->
            invalidArg
                "path"
                $"Unsupported validation-package script extension: {extension}"

    let fromFile (repoPath: string) (lastUpdated: DateTimeOffset) =
        let language = languageFromPath repoPath

        let metadata =
            repoPath
            |> File.ReadAllText
            |> ValidationPackageYaml.extractOrFail language

        create
            repoPath
            (Path.GetFileName(repoPath))
            lastUpdated
            (ContentHash.ofFile repoPath)
            metadata

    let identityEquals first second =
        ValidationPackageMetadata.identityEquals(first.Metadata, second.Metadata)

    let contentEquals first second =
        first.ContentHash = second.ContentHash

    let tryGetSemanticVersion stagedPackage =
        ValidationPackageMetadata.tryGetSemanticVersion stagedPackage.Metadata

    let getSemanticVersion stagedPackage =
        ValidationPackageMetadata.getSemanticVersion stagedPackage.Metadata

    let tryGetSemanticVersionString stagedPackage =
        ValidationPackageMetadata.tryGetSemanticVersionString stagedPackage.Metadata

    let getSemanticVersionString stagedPackage =
        ValidationPackageMetadata.getSemanticVersionString stagedPackage.Metadata
