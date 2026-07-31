module PackageTasks

open BlackFox.Fake
open Fake.DotNet

open System
open System.IO
open System.Text.RegularExpressions

open Helpers
open ProjectInfo
open BasicTasks

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

let private packageVersion project =
    let matched =
        Regex.Match(
            File.ReadAllText project,
            "<PackageVersion>([^<]+)</PackageVersion>"
        )

    if not matched.Success then
        failwithf "PackageVersion is missing from %s" project

    matched.Groups[1].Value

let private pythonPackageVersion (version: string) =
    version
        .Replace("-alpha.", "a")
        .Replace("-beta.", "b")
        .Replace("-preview.", "a")
        .Replace("-rc.", "rc")

let private modelVersion = packageVersion modelProject
let private codecsVersion = packageVersion codecsProject

let private removeFableModulesGitIgnore outputDirectory =
    let path = Path.Combine(outputDirectory, "fable_modules", ".gitignore")

    if File.Exists path then
        File.Delete path

let private transpile project language outputDirectory =
    recreateDirectory outputDirectory
    runDotNet
        "fable"
        $"{project} --lang {language} --outDir \"{outputDirectory}\" --noCache"
        "."
    removeFableModulesGitIgnore outputDirectory

let private writeVersionedJavaScriptManifest sourceDirectory version outputDirectory =
    let source = Path.Combine(sourceDirectory, "package.json")
    let target = Path.Combine(outputDirectory, "package.json")
    let content =
        Regex("\"version\"\\s*:\\s*\"[^\"]+\"")
            .Replace(File.ReadAllText source, $"\"version\": \"{version}\"", 1)

    let content =
        Regex("\"@nfdi4plants/validationpackage-model\"\\s*:\\s*\"[^\"]+\"")
            .Replace(content, $"\"@nfdi4plants/validationpackage-model\": \"{modelVersion}\"", 1)

    File.WriteAllText(target, content)
    File.Copy(Path.Combine(sourceDirectory, "index.js"), Path.Combine(outputDirectory, "index.js"), true)

let private writePythonBuildProject outputDirectory distributionName packageName version description dependencies =
    let dependencyText =
        dependencies
        |> List.map (fun dependency -> $"    \"{dependency}\",")
        |> String.concat Environment.NewLine

    let pyproject =
        $"""[project]
name = "{distributionName}"
version = "{pythonPackageVersion version}"
description = "{description}"
license = "MIT"
requires-python = ">=3.12"
dependencies = [
{dependencyText}
]

[project.urls]
Homepage = "https://github.com/nfdi4plants/arc-validate-package-registry"
Repository = "https://github.com/nfdi4plants/arc-validate-package-registry.git"

[build-system]
requires = ["hatchling"]
build-backend = "hatchling.build"

[tool.hatch.build.targets.wheel]
packages = ["{packageName}"]
exclude = [
    "**/*.fs",
    "**/*.fsproj",
    "**/*.fableproj",
    "**/obj/**",
    "**/pyproject.toml",
    "**/*.md",
]
"""

    File.WriteAllText(Path.Combine(outputDirectory, "pyproject.toml"), pyproject)

let private rewriteFiles outputDirectory (pattern: string) (replacement: string) =
    Directory.EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories)
    |> Seq.filter (fun path -> path.EndsWith(".js") || path.EndsWith(".py"))
    |> Seq.iter (fun path ->
        let source = File.ReadAllText path
        let rewritten = Regex.Replace(source, pattern, replacement)

        if rewritten <> source then
            File.WriteAllText(path, rewritten)
    )

let private deleteDirectoryIfPresent path =
    if Directory.Exists path then
        Directory.Delete(path, true)

let private packModelJavaScript () =
    let outputDirectory = Path.Combine(portableArtifactsDir, "validationpackage-model", "javascript")
    transpile modelProject "javascript" outputDirectory
    writeVersionedJavaScriptManifest (Path.Combine("src", "ValidationPackage.Model")) modelVersion outputDirectory
    runNpm [ "pack"; Path.GetFullPath outputDirectory; "--pack-destination"; Path.GetFullPath packageDir ] "."

let private packModelPython () =
    let outputDirectory = Path.Combine(portableArtifactsDir, "validationpackage-model", "python")
    let packageDirectory = Path.Combine(outputDirectory, "validation_package_model")
    recreateDirectory outputDirectory
    runDotNet
        "fable"
        $"{modelProject} --lang python --outDir \"{packageDirectory}\" --noCache"
        "."
    removeFableModulesGitIgnore packageDirectory
    File.Copy(
        Path.Combine("src", "ValidationPackage.Model", "__init__.py"),
        Path.Combine(packageDirectory, "__init__.py"),
        true
    )
    writePythonBuildProject
        outputDirectory
        "validationpackage-model"
        "validation_package_model"
        modelVersion
        "Portable metadata and CWL input model for ARC validation packages."
        [ "fable-library==5.11.0" ]
    runUv [ "build"; "--wheel"; "--out-dir"; Path.GetFullPath packageDir; Path.GetFullPath outputDirectory ] "."

let private externalizeCodecsModelJavaScript outputDirectory =
    rewriteFiles
        outputDirectory
        "\"(?:\\.\\./)+ValidationPackage\\.Model/([^\"]+)\""
        "\"@nfdi4plants/validationpackage-model/$1\""
    deleteDirectoryIfPresent (Path.Combine(outputDirectory, "ValidationPackage.Model"))

let private externalizeCodecsModelPython packageDirectory =
    rewriteFiles
        packageDirectory
        @"from \.{2,}ValidationPackage_Model\."
        "from validation_package_model."
    deleteDirectoryIfPresent (Path.Combine(packageDirectory, "ValidationPackage_Model"))

let private packCodecsJavaScript () =
    let outputDirectory = Path.Combine(portableArtifactsDir, "validationpackage-codecs", "javascript")
    transpile codecsProject "javascript" outputDirectory
    externalizeCodecsModelJavaScript outputDirectory
    writeVersionedJavaScriptManifest (Path.Combine("src", "ValidationPackage.Codecs")) codecsVersion outputDirectory
    runNpm [ "pack"; Path.GetFullPath outputDirectory; "--pack-destination"; Path.GetFullPath packageDir ] "."

let private packCodecsPython () =
    let outputDirectory = Path.Combine(portableArtifactsDir, "validationpackage-codecs", "python")
    let packageDirectory = Path.Combine(outputDirectory, "validation_package_codecs")
    recreateDirectory outputDirectory
    runDotNet
        "fable"
        $"{codecsProject} --lang python --outDir \"{packageDirectory}\" --noCache"
        "."
    removeFableModulesGitIgnore packageDirectory
    externalizeCodecsModelPython packageDirectory
    File.Copy(
        Path.Combine("src", "ValidationPackage.Codecs", "__init__.py"),
        Path.Combine(packageDirectory, "__init__.py"),
        true
    )
    writePythonBuildProject
        outputDirectory
        "validationpackage-codecs"
        "validation_package_codecs"
        codecsVersion
        "Portable YAML frontmatter and JSON codecs for ARC validation packages."
        [
            "fable-library==5.11.0"
            $"validationpackage-model=={pythonPackageVersion modelVersion}"
        ]
    runUv [ "build"; "--wheel"; "--out-dir"; Path.GetFullPath packageDir; Path.GetFullPath outputDirectory ] "."

let packClient = BuildTask.create "PackClient" [ cleanPackages ] {
    packProject clientProject
}

let packClientInterop = BuildTask.create "PackClientInterop" [ cleanPackages ] {
    packProject clientInteropProject
}

let packModel = BuildTask.create "PackModel" [ cleanPackages; preparePortableToolchain ] {
    packProject modelProject
    packModelJavaScript ()
    packModelPython ()
}

let packCodecs = BuildTask.create "PackCodecs" [ cleanPackages; preparePortableToolchain ] {
    packProject codecsProject
    packCodecsJavaScript ()
    packCodecsPython ()
}

let packClientPackages =
    BuildTask.createEmpty "PackClientPackages" [ packClient; packModel; packClientInterop ]

let packPortablePackages =
    BuildTask.createEmpty "PackPortablePackages" [ packModel; packCodecs ]
