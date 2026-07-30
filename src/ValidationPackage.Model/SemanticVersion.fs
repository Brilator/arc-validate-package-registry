namespace ValidationPackage.Model

open System
open Fable.Core

module private SemanticVersionParsing =

    let isAsciiDigit (character: char) =
        character >= '0' && character <= '9'

    let isAsciiLetter (character: char) =
        (character >= 'a' && character <= 'z')
        || (character >= 'A' && character <= 'Z')

    let isIdentifierCharacter (character: char) =
        isAsciiDigit character
        || isAsciiLetter character
        || character = '-'

    let isNonEmptyIdentifier (value: string) =
        value.Length > 0 && value |> Seq.forall isIdentifierCharacter

    let hasValidNumericLeadingZero (value: string) =
        not (
            value.Length > 1
            && value |> Seq.forall isAsciiDigit
            && value[0] = '0'
        )

    let isValidPreReleaseIdentifier (value: string) =
        isNonEmptyIdentifier value && hasValidNumericLeadingZero value

    let isValidBuildIdentifier (value: string) =
        isNonEmptyIdentifier value

    let trySplitOnce (separator: string) (value: string) =
        let firstIndex = value.IndexOf(separator, StringComparison.Ordinal)

        if firstIndex < 0 then
            Some(value, "")
        elif value.IndexOf(separator, firstIndex + separator.Length, StringComparison.Ordinal) >= 0 then
            None
        else
            Some(
                value.Substring(0, firstIndex),
                value.Substring(firstIndex + separator.Length)
            )

    let splitAtFirst (separator: string) (value: string) =
        let index = value.IndexOf(separator, StringComparison.Ordinal)

        if index < 0 then
            value, ""
        else
            value.Substring(0, index), value.Substring(index + separator.Length)

    let tryParseCoreNumber (value: string) =
        if
            value.Length = 0
            || value |> Seq.exists (isAsciiDigit >> not)
            || (value.Length > 1 && value[0] = '0')
        then
            None
        else
            match Int32.TryParse(value) with
            | true, parsed -> Some parsed
            | false, _ -> None

    let hasValidIdentifiers validator (value: string) =
        value.Split('.')
        |> Array.forall validator

[<AttachMembers>]
type SemVer() =

    let mutable _major = -1
    let mutable _minor = -1
    let mutable _patch = -1
    let mutable _preRelease = ""
    let mutable _buildMetadata = ""

    member _.Major
        with get () = _major
        and set value = _major <- value

    member _.Minor
        with get () = _minor
        and set value = _minor <- value

    member _.Patch
        with get () = _patch
        and set value = _patch <- value

    member _.PreRelease
        with get () = _preRelease
        and set value = _preRelease <- value

    member _.BuildMetadata
        with get () = _buildMetadata
        and set value = _buildMetadata <- value

    override this.GetHashCode() =
        SemVer.getHashCode(this)

    static member getHashCode(semVer: SemVer) =
        PortableHash.combineValues [
            semVer.Major
            semVer.Minor
            semVer.Patch
            PortableHash.stringValue semVer.PreRelease
            PortableHash.stringValue semVer.BuildMetadata
        ]

    override this.Equals(other) =
        match other with
        | :? SemVer as semVer ->
            (
                this.Major,
                this.Minor,
                this.Patch,
                this.PreRelease,
                this.BuildMetadata
            ) = (
                semVer.Major,
                semVer.Minor,
                semVer.Patch,
                semVer.PreRelease,
                semVer.BuildMetadata
            )
        | _ -> false

    static member create (
        major: int,
        minor: int,
        patch: int,
        ?PreRelease: string,
        ?BuildMetadata: string
    ) =
        let semVer =
            SemVer(
                Major = major,
                Minor = minor,
                Patch = patch
            )

        PreRelease |> Option.iter (fun value -> semVer.PreRelease <- value)
        BuildMetadata |> Option.iter (fun value -> semVer.BuildMetadata <- value)
        semVer

    static member tryParse(version: string) =
        if
            isNull version
            || version.EndsWith("-", StringComparison.Ordinal)
            || version.EndsWith("+", StringComparison.Ordinal)
        then
            None
        else
            match SemanticVersionParsing.trySplitOnce "+" version with
            | None -> None
            | Some(versionWithoutBuild, buildMetadata) ->
                let coreVersion, preRelease =
                    SemanticVersionParsing.splitAtFirst "-" versionWithoutBuild

                let coreParts = coreVersion.Split('.')

                let validPreRelease =
                    preRelease = ""
                    ||
                    SemanticVersionParsing.hasValidIdentifiers
                        SemanticVersionParsing.isValidPreReleaseIdentifier
                        preRelease

                let validBuildMetadata =
                    buildMetadata = ""
                    ||
                    SemanticVersionParsing.hasValidIdentifiers
                        SemanticVersionParsing.isValidBuildIdentifier
                        buildMetadata

                if coreParts.Length <> 3 || not validPreRelease || not validBuildMetadata then
                    None
                else
                    match
                        SemanticVersionParsing.tryParseCoreNumber coreParts[0],
                        SemanticVersionParsing.tryParseCoreNumber coreParts[1],
                        SemanticVersionParsing.tryParseCoreNumber coreParts[2]
                    with
                    | Some major, Some minor, Some patch ->
                        Some(
                            SemVer.create(
                                major,
                                minor,
                                patch,
                                preRelease,
                                buildMetadata
                            )
                        )
                    | _ -> None

    static member toString(semVer: SemVer) =
        match semVer.PreRelease, semVer.BuildMetadata with
        | preRelease, buildMetadata when preRelease <> "" && buildMetadata <> "" ->
            $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}-{preRelease}+{buildMetadata}"
        | preRelease, _ when preRelease <> "" ->
            $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}-{preRelease}"
        | _, buildMetadata when buildMetadata <> "" ->
            $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}+{buildMetadata}"
        | _ ->
            $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}"
