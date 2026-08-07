module ReleaseMetadata

open System
open System.Globalization
open System.IO
open System.Text.RegularExpressions
open System.Xml.Linq

open ProjectInfo

type private ReleaseDefinition = {
    Name: string
    ProjectPath: string
    VersionProperty: string
    ReleaseNotesPath: string
}

let private releaseDefinitions =
    [
        {
            Name = "ValidationPackage.Model"
            ProjectPath = modelProject
            VersionProperty = "PackageVersion"
            ReleaseNotesPath = "src/ValidationPackage.Model/RELEASE_NOTES.md"
        }
        {
            Name = "ValidationPackage.Codecs"
            ProjectPath = codecsProject
            VersionProperty = "PackageVersion"
            ReleaseNotesPath = "src/ValidationPackage.Codecs/RELEASE_NOTES.md"
        }
        {
            Name = "AVPRClient"
            ProjectPath = clientProject
            VersionProperty = "PackageVersion"
            ReleaseNotesPath = "src/AVPRClient/RELEASE_NOTES.md"
        }
        {
            Name = "AVPRClient.Interop"
            ProjectPath = clientInteropProject
            VersionProperty = "PackageVersion"
            ReleaseNotesPath = "src/AVPRClient.Interop/RELEASE_NOTES.md"
        }
        {
            Name = "PackageRegistryService"
            ProjectPath = "src/PackageRegistryService/PackageRegistryService.csproj"
            VersionProperty = "Version"
            ReleaseNotesPath = "src/PackageRegistryService/RELEASE_NOTES.md"
        }
    ]

let private versionHeadingCandidate =
    Regex(@"^#{1,6}\s+\[?v?\d+\.\d+\.\d+", RegexOptions.Compiled)

let private canonicalReleaseHeading =
    Regex(
        @"^## (?<version>\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?) - (?<date>\d{4}-\d{2}-\d{2})(?: - \S.*)?$",
        RegexOptions.Compiled
    )

let private readProjectVersion definition =
    let document = XDocument.Load(definition.ProjectPath)

    let values =
        document.Descendants(XName.Get(definition.VersionProperty))
        |> Seq.map (fun element -> element.Value.Trim())
        |> Seq.toArray

    match values with
    | [| version |] when not (String.IsNullOrWhiteSpace version) -> version
    | [||] ->
        failwithf
            "%s does not define the authoritative <%s> in %s."
            definition.Name
            definition.VersionProperty
            definition.ProjectPath
    | _ ->
        failwithf
            "%s must define exactly one authoritative <%s> in %s."
            definition.Name
            definition.VersionProperty
            definition.ProjectPath

let private readLatestRelease definition =
    let heading =
        File.ReadLines(definition.ReleaseNotesPath)
        |> Seq.tryFind versionHeadingCandidate.IsMatch
        |> Option.defaultWith (fun () ->
            failwithf
                "%s has no version heading in %s."
                definition.Name
                definition.ReleaseNotesPath)

    let matched = canonicalReleaseHeading.Match heading

    if not matched.Success then
        failwithf
            "%s latest release heading must use '## <version> - YYYY-MM-DD', with an optional ' - <title>' suffix. Found: %s"
            definition.Name
            heading

    let date = matched.Groups["date"].Value
    let mutable parsedDate = DateTime.MinValue

    if
        not (
            DateTime.TryParseExact(
                date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                &parsedDate
            )
        )
    then
        failwithf "%s latest release heading contains an invalid date: %s" definition.Name date

    matched.Groups["version"].Value

let validate () =
    releaseDefinitions
    |> List.iter (fun definition ->
        let projectVersion = readProjectVersion definition
        let releaseNotesVersion = readLatestRelease definition

        if projectVersion <> releaseNotesVersion then
            failwithf
                "%s project version '%s' does not match its latest release-notes version '%s'."
                definition.Name
                projectVersion
                releaseNotesVersion

        printfn "%s release metadata: %s" definition.Name projectVersion)
