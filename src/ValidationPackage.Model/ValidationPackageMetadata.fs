namespace ValidationPackage.Model

open Fable.Core

[<AttachMembers>]
type ValidationPackageMetadata() =

    let mutable _name = ""
    let mutable _summary = ""
    let mutable _description = ""
    let mutable _majorVersion = -1
    let mutable _minorVersion = -1
    let mutable _patchVersion = -1
    let mutable _preReleaseVersionSuffix = ""
    let mutable _buildMetadataVersionSuffix = ""
    let mutable _programmingLanguage = ""
    let mutable _publish = false
    let mutable _authors: Author array = Array.empty
    let mutable _tags: OntologyAnnotation array = Array.empty
    let mutable _releaseNotes = ""
    let mutable _cqcHookEndpoint = ""
    let mutable _inputs: CommandInputParameter array = Array.empty

    member _.Name
        with get () = _name
        and set value = _name <- value

    member _.Summary
        with get () = _summary
        and set value = _summary <- value

    member _.Description
        with get () = _description
        and set value = _description <- value

    member _.MajorVersion
        with get () = _majorVersion
        and set value = _majorVersion <- value

    member _.MinorVersion
        with get () = _minorVersion
        and set value = _minorVersion <- value

    member _.PatchVersion
        with get () = _patchVersion
        and set value = _patchVersion <- value

    member _.PreReleaseVersionSuffix
        with get () = _preReleaseVersionSuffix
        and set value = _preReleaseVersionSuffix <- value

    member _.BuildMetadataVersionSuffix
        with get () = _buildMetadataVersionSuffix
        and set value = _buildMetadataVersionSuffix <- value

    member _.ProgrammingLanguage
        with get () = _programmingLanguage
        and set value = _programmingLanguage <- value

    member _.Publish
        with get () = _publish
        and set value = _publish <- value

    member _.Authors
        with get () = _authors
        and set value = _authors <- value

    member _.Tags
        with get () = _tags
        and set value = _tags <- value

    member _.ReleaseNotes
        with get () = _releaseNotes
        and set value = _releaseNotes <- value

    member _.CQCHookEndpoint
        with get () = _cqcHookEndpoint
        and set value = _cqcHookEndpoint <- value

    member _.Inputs
        with get () = _inputs
        and set value = _inputs <- value

    override this.GetHashCode() =
        ValidationPackageMetadata.getHashCode(this)

    static member getHashCode(metadata: ValidationPackageMetadata) =
        PortableHash.combineValues [
            PortableHash.stringValue metadata.Name
            PortableHash.stringValue metadata.Summary
            PortableHash.stringValue metadata.Description
            metadata.MajorVersion
            metadata.MinorVersion
            metadata.PatchVersion
            PortableHash.stringValue metadata.PreReleaseVersionSuffix
            PortableHash.stringValue metadata.BuildMetadataVersionSuffix
            PortableHash.stringValue metadata.ProgrammingLanguage
            PortableHash.boolValue metadata.Publish
            PortableHash.arrayValue Author.getHashCode metadata.Authors
            PortableHash.arrayValue OntologyAnnotation.getHashCode metadata.Tags
            PortableHash.stringValue metadata.ReleaseNotes
            PortableHash.stringValue metadata.CQCHookEndpoint
            PortableHash.arrayValue CommandInputParameter.getHashCode metadata.Inputs
        ]

    override this.Equals(other) =
        match other with
        | :? ValidationPackageMetadata as metadata ->
            (
                this.Name,
                this.Summary,
                this.Description,
                this.MajorVersion,
                this.MinorVersion,
                this.PatchVersion,
                this.PreReleaseVersionSuffix,
                this.BuildMetadataVersionSuffix,
                this.ProgrammingLanguage,
                this.Publish,
                this.Authors,
                this.Tags,
                this.ReleaseNotes,
                this.CQCHookEndpoint,
                this.Inputs
            ) = (
                metadata.Name,
                metadata.Summary,
                metadata.Description,
                metadata.MajorVersion,
                metadata.MinorVersion,
                metadata.PatchVersion,
                metadata.PreReleaseVersionSuffix,
                metadata.BuildMetadataVersionSuffix,
                metadata.ProgrammingLanguage,
                metadata.Publish,
                metadata.Authors,
                metadata.Tags,
                metadata.ReleaseNotes,
                metadata.CQCHookEndpoint,
                metadata.Inputs
            )
        | _ -> false

    static member create (
        name: string,
        summary: string,
        description: string,
        majorVersion: int,
        minorVersion: int,
        patchVersion: int,
        programmingLanguage: string,
        ?PreReleaseVersionSuffix: string,
        ?BuildMetadataVersionSuffix: string,
        ?Publish: bool,
        ?Authors: Author array,
        ?Tags: OntologyAnnotation array,
        ?ReleaseNotes: string,
        ?CQCHookEndpoint: string,
        ?Inputs: CommandInputParameter array
    ) =
        let metadata =
            ValidationPackageMetadata(
                Name = name,
                Summary = summary,
                Description = description,
                MajorVersion = majorVersion,
                MinorVersion = minorVersion,
                PatchVersion = patchVersion,
                ProgrammingLanguage = programmingLanguage
            )

        PreReleaseVersionSuffix
        |> Option.iter (fun value -> metadata.PreReleaseVersionSuffix <- value)

        BuildMetadataVersionSuffix
        |> Option.iter (fun value -> metadata.BuildMetadataVersionSuffix <- value)

        Publish |> Option.iter (fun value -> metadata.Publish <- value)
        Authors |> Option.iter (fun value -> metadata.Authors <- value)
        Tags |> Option.iter (fun value -> metadata.Tags <- value)
        ReleaseNotes |> Option.iter (fun value -> metadata.ReleaseNotes <- value)
        CQCHookEndpoint |> Option.iter (fun value -> metadata.CQCHookEndpoint <- value)
        Inputs |> Option.iter (fun value -> metadata.Inputs <- value)
        metadata

    static member tryGetSemanticVersion(metadata: ValidationPackageMetadata) =
        SemVer.create(
            metadata.MajorVersion,
            metadata.MinorVersion,
            metadata.PatchVersion,
            metadata.PreReleaseVersionSuffix,
            metadata.BuildMetadataVersionSuffix
        )
        |> SemVer.toString
        |> SemVer.tryParse

    static member getSemanticVersion(metadata: ValidationPackageMetadata) =
        metadata
        |> ValidationPackageMetadata.tryGetSemanticVersion
        |> Option.get

    static member tryGetSemanticVersionString(metadata: ValidationPackageMetadata) =
        metadata
        |> ValidationPackageMetadata.tryGetSemanticVersion
        |> Option.map SemVer.toString

    static member getSemanticVersionString(metadata: ValidationPackageMetadata) =
        metadata
        |> ValidationPackageMetadata.tryGetSemanticVersionString
        |> Option.get

    static member tryGetIdentity(metadata: ValidationPackageMetadata) =
        metadata
        |> ValidationPackageMetadata.tryGetSemanticVersion
        |> Option.map (fun version ->
            ValidationPackageIdentity.create(metadata.Name, version)
        )

    static member getIdentity(metadata: ValidationPackageMetadata) =
        metadata
        |> ValidationPackageMetadata.tryGetIdentity
        |> Option.get

    static member identityEquals (
        first: ValidationPackageMetadata,
        second: ValidationPackageMetadata
    ) =
        match
            ValidationPackageMetadata.tryGetIdentity first,
            ValidationPackageMetadata.tryGetIdentity second
        with
        | Some firstIdentity, Some secondIdentity ->
            firstIdentity = secondIdentity
        | _ ->
            false
