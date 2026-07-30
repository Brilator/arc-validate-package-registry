namespace AVPR.Staging

[<RequireQualifiedAccess>]
module StagingConstants =

    let [<Literal>] RelativePath = "StagingArea"

    // Kept at the staging boundary for filename validation. Semantic-version
    // values themselves are parsed by ValidationPackage.Model.SemVer.
    let [<Literal>] SemanticVersionPattern =
        @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$"
