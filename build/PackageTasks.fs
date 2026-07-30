module PackageTasks

open BlackFox.Fake
open Fake.DotNet

open Helpers
open ProjectInfo

let cleanPackages = BuildTask.create "CleanPackages" [] {
    recreateDirectory packageDir
}

let private packProject project =
    ensureDirectory packageDir

    project
    |> DotNet.pack (fun options ->
        { options with
            Configuration = DotNet.BuildConfiguration.Release
            OutputPath = Some packageDir
            MSBuildParams =
                { options.MSBuildParams with
                    DisableInternalBinLog = true } })

let packClient = BuildTask.create "PackClient" [ cleanPackages ] {
    packProject clientProject
}

let packClientInterop = BuildTask.create "PackClientInterop" [ cleanPackages ] {
    packProject clientInteropProject
}

let packModel = BuildTask.create "PackModel" [ cleanPackages ] {
    packProject modelProject
}

let packCodecs = BuildTask.create "PackCodecs" [ cleanPackages ] {
    packProject codecsProject
}

let packClientPackages =
    BuildTask.createEmpty "PackClientPackages" [ packClient; packModel; packClientInterop ]

let packPortablePackages =
    BuildTask.createEmpty "PackPortablePackages" [ packModel; packCodecs ]
