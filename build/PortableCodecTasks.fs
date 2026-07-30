module PortableCodecTasks

open BlackFox.Fake
open Fake.DotNet

open System.IO

open Helpers
open ProjectInfo
open BasicTasks
open PackageTasks

let private codecsTestsDir = Path.Combine(portableArtifactsDir, "codec-tests")
let private codecsPackageSmokeDir = Path.Combine(portableArtifactsDir, "codec-package-smoke")

let private fable project language outputDirectory =
    runDotNet
        "fable"
        $"{project} --outDir \"{outputDirectory}\" --lang {language} --noCache"
        "."

let testCodecsDotNet =
    BuildTask.create "TestCodecsDotNet" [ cleanPortableArtifacts; preparePortableToolchain ] {
        runDotNet
            "run"
            $"--project {codecsTestsProject} --configuration {configuration}"
            "."
    }

let testCodecsJavaScript =
    BuildTask.create "TestCodecsJavaScript" [ testCodecsDotNet ] {
        let outputDirectory = Path.Combine(codecsTestsDir, "javascript", "out")
        fable codecsJavaScriptTestsProject "javascript" outputDirectory
        runCommand "node" [ Path.Combine(codecsTestsDir, "javascript", "Main.js") ] "."
    }

let testCodecsPython =
    BuildTask.create "TestCodecsPython" [ testCodecsJavaScript ] {
        let outputDirectory = Path.Combine(codecsTestsDir, "python", "out")
        let entryPoint = Path.Combine(outputDirectory, "__", "main.py")
        let nestedOutputDirectory = Path.Combine(outputDirectory, "__")
        let pythonPath (path: string) = path.Replace('\\', '/')
        let command =
            sprintf
                "import runpy,sys; sys.path[:0] = [%A, %A]; runpy.run_path(%A, run_name='__main__')"
                (pythonPath outputDirectory)
                (pythonPath nestedOutputDirectory)
                (pythonPath entryPoint)

        fable codecsPythonTestsProject "python" outputDirectory
        runUv [ "run"; "--locked"; "python"; "-c"; command ] "."
    }

let testCodecsPackage =
    BuildTask.create "TestCodecsPackage" [ testCodecsPython; packPortablePackages ] {
        let cacheDirectory = Path.Combine(packageCacheDir, "codecs") |> Path.GetFullPath
        let nugetConfig =
            writeNuGetConfig
                (Path.Combine(portableArtifactsDir, "codec-package-smoke.NuGet.config"))
                packageDir
        recreateDirectory cacheDirectory

        codecsPackageSmokeProject
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
            $"--project {codecsPackageSmokeProject} --configuration {configuration} --no-restore"
            "."

        let javaScriptOutput = Path.Combine(codecsPackageSmokeDir, "javascript")
        fable codecsPackageSmokeProject "javascript" javaScriptOutput
        runCommand "node" [ Path.Combine(javaScriptOutput, "Program.js") ] "."

        let pythonOutput = Path.Combine(codecsPackageSmokeDir, "python")
        fable codecsPackageSmokeProject "python" pythonOutput
        runUv [ "run"; "--locked"; "python"; Path.Combine(pythonOutput, "program.py") ] "."
    }

let testPortableCodecs =
    BuildTask.createEmpty "TestPortableCodecs" [ testCodecsPackage ]
