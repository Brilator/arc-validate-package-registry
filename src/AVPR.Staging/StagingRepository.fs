namespace AVPR.Staging

open System
open System.IO

type StagingRepository =

    static member private normalizePath(path: string) =
        path.Replace(Path.DirectorySeparatorChar, '/')

    static member discover(?RepoRoot: string) =
        let stagingPath =
            RepoRoot
            |> Option.map (fun root ->
                Path.Combine(root, StagingConstants.RelativePath)
            )
            |> Option.defaultValue StagingConstants.RelativePath

        let discoverPattern pattern =
            Directory.GetFiles(stagingPath, pattern, SearchOption.AllDirectories)
            |> Array.map (fun path ->
                StagedValidationPackage.fromFile
                    (StagingRepository.normalizePath path)
                    (DateTimeOffsetNormalization.truncateToSeconds DateTimeOffset.Now)
            )

        Array.concat [
            discoverPattern "*.fsx"
            discoverPattern "*.py"
        ]
