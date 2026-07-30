module PortableModelTasks

open BlackFox.Fake
open Fake.DotNet

open System.IO

open Helpers
open ProjectInfo
open BasicTasks
open PackageTasks

let private modelTestsDir = Path.Combine(portableArtifactsDir, "model-tests")
let private modelPackageSmokeDir = Path.Combine(portableArtifactsDir, "model-package-smoke")

let private fable project language outputDirectory =
    runDotNet
        "fable"
        $"{project} --outDir \"{outputDirectory}\" --lang {language} --noCache"
        "."

let testModelDotNet =
    BuildTask.create "TestModelDotNet" [ cleanPortableArtifacts; preparePortableToolchain ] {
        runDotNet
            "run"
            $"--project {modelTestsProject} --configuration {configuration}"
            "."
    }

let testModelJavaScript =
    BuildTask.create "TestModelJavaScript" [ testModelDotNet ] {
        let outputDirectory = Path.Combine(modelTestsDir, "javascript")
        fable modelTestsProject "javascript" outputDirectory
        runCommand "node" [ Path.Combine(outputDirectory, "Main.js") ] "."
    }

let testModelPython =
    BuildTask.create "TestModelPython" [ testModelJavaScript ] {
        let outputDirectory = Path.Combine(modelTestsDir, "python")
        fable modelTestsProject "python" outputDirectory
        runUv [ "run"; "--locked"; "python"; Path.Combine(outputDirectory, "main.py") ] "."
    }

let testModelPackage =
    BuildTask.create "TestModelPackage" [ testModelPython; packModel ] {
        let cacheDirectory = Path.Combine(packageCacheDir, "model") |> Path.GetFullPath
        let nugetConfig =
            writeNuGetConfig
                (Path.Combine(portableArtifactsDir, "model-package-smoke.NuGet.config"))
                packageDir
        recreateDirectory cacheDirectory

        modelPackageSmokeProject
        |> DotNet.restore (fun options ->
            { options with
                ConfigFile = Some nugetConfig
                Packages = [ cacheDirectory ]
                NoCache = true
                MSBuildParams =
                    { options.MSBuildParams with
                        DisableInternalBinLog = true } })

        runDotNet
            "run"
            $"--project {modelPackageSmokeProject} --configuration {configuration} --no-restore"
            "."

        let javaScriptOutput = Path.Combine(modelPackageSmokeDir, "javascript")
        fable modelPackageSmokeProject "javascript" javaScriptOutput
        runCommand "node" [ Path.Combine(javaScriptOutput, "Program.js") ] "."

        let pythonOutput = Path.Combine(modelPackageSmokeDir, "python")
        fable modelPackageSmokeProject "python" pythonOutput
        runUv [ "run"; "--locked"; "python"; Path.Combine(pythonOutput, "program.py") ] "."
    }

let testPortableModel =
    BuildTask.createEmpty "TestPortableModel" [ testModelPackage ]
