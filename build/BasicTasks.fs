module BasicTasks

open BlackFox.Fake
open Fake.DotNet

open Helpers
open ProjectInfo

let validateReleaseMetadata = BuildTask.create "ValidateReleaseMetadata" [] {
    ReleaseMetadata.validate ()
}

let cleanPortableArtifacts = BuildTask.create "CleanPortableArtifacts" [] {
    recreateDirectory portableArtifactsDir
    recreateDirectory packageCacheDir
}

let preparePortableToolchain = BuildTask.create "PreparePortableToolchain" [] {
    runDotNet "tool" "restore" "."
    runUv [ "sync"; "--locked" ] "."
}

let buildSolution = BuildTask.create "BuildSolution" [ validateReleaseMetadata ] {
    mainSolution
    |> DotNet.build (fun options ->
        { options with
            Configuration = DotNet.BuildConfiguration.Release
            MSBuildParams =
                { options.MSBuildParams with
                    DisableInternalBinLog = true } })
}

let testSolution = BuildTask.create "TestSolution" [ buildSolution ] {
    mainSolution
    |> DotNet.test (fun options ->
        { options with
            Configuration = DotNet.BuildConfiguration.Release
            NoBuild = true
            MSBuildParams =
                { options.MSBuildParams with
                    DisableInternalBinLog = true } })
}

let testStagingArea = BuildTask.create "TestStagingArea" [ validateReleaseMetadata ] {
    stagingSolution
    |> DotNet.build (fun options ->
        { options with
            Configuration = DotNet.BuildConfiguration.Release
            MSBuildParams =
                { options.MSBuildParams with
                    DisableInternalBinLog = true } })

    stagingSolution
    |> DotNet.test (fun options ->
        { options with
            Configuration = DotNet.BuildConfiguration.Release
            NoBuild = true
            MSBuildParams =
                { options.MSBuildParams with
                    DisableInternalBinLog = true } })
}
