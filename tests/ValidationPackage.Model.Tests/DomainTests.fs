module ValidationPackage.Model.Tests.DomainTests

open Fable.Pyxpecto
open ValidationPackage.Model
open ValidationPackage.Model.Tests.ReferenceObjects

let private copyMetadata (source: ValidationPackageMetadata) =
    ValidationPackageMetadata.create(
        name = source.Name,
        summary = source.Summary,
        description = source.Description,
        majorVersion = source.MajorVersion,
        minorVersion = source.MinorVersion,
        patchVersion = source.PatchVersion,
        programmingLanguage = source.ProgrammingLanguage,
        PreReleaseVersionSuffix = source.PreReleaseVersionSuffix,
        BuildMetadataVersionSuffix = source.BuildMetadataVersionSuffix,
        Publish = source.Publish,
        Authors = source.Authors,
        Tags = source.Tags,
        ReleaseNotes = source.ReleaseNotes,
        CQCHookEndpoint = source.CQCHookEndpoint,
        Inputs = source.Inputs
    )

let tests =
    testList "validation-package domain" [
        testCase "author factories preserve mandatory and optional fields" <| fun () ->
            let mandatory = Author.create("Test Author")

            let allFields =
                Author.create(
                    "Test Author",
                    Email = "test@example.org",
                    Affiliation = "DataPLANT",
                    AffiliationLink = "https://nfdi4plants.org"
                )

            Expect.equal mandatory.FullName "Test Author" "Mandatory name"
            Expect.equal mandatory.Email "" "Email default"
            Expect.equal allFields.Email "test@example.org" "Optional email"
            Expect.equal allFields.Affiliation "DataPLANT" "Optional affiliation"

        testCase "ontology annotation factories preserve defaults and optional fields" <| fun () ->
            let mandatory = OntologyAnnotation.create("validation")

            let allFields =
                OntologyAnnotation.create(
                    "validation",
                    TermSourceRef = "AVPR",
                    TermAccessionNumber = "AVPR:validation"
                )

            Expect.equal mandatory.Name "validation" "Mandatory annotation name"
            Expect.equal mandatory.TermSourceREF "" "Source default"
            Expect.equal allFields.TermSourceREF "AVPR" "Optional source"
            Expect.equal allFields.TermAccessionNumber "AVPR:validation" "Optional accession"

        testCase "metadata factory preserves mandatory defaults" <| fun () ->
            let actual =
                ValidationPackageMetadata.create(
                    name = "name",
                    summary = "summary",
                    description = "description",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    programmingLanguage = "FSharp"
                )

            Expect.equal actual Metadata.mandatory "Mandatory metadata should match"
            Expect.isFalse actual.Publish "Publish default"
            Expect.isEmpty actual.Authors "Authors default"
            Expect.isEmpty actual.Tags "Tags default"
            Expect.isEmpty actual.Inputs "Inputs default"

        testCase "metadata factory preserves all fields and structural equality" <| fun () ->
            let copied = copyMetadata Metadata.allFields
            Expect.equal copied Metadata.allFields "All metadata fields should participate"
            Expect.equal (ValidationPackageMetadata.getHashCode copied) (ValidationPackageMetadata.getHashCode Metadata.allFields) "Equivalent metadata hashes equally"

        testCase "metadata semantic-version helpers preserve suffixes" <| fun () ->
            let actual = ValidationPackageMetadata.tryGetSemanticVersion Metadata.allFields
            Expect.isSome actual "All-fields metadata should have a valid semantic version"

            Expect.equal
                actual.Value
                SemanticVersions.prereleaseAndBuildMetadata
                "Semantic-version components should match"

            Expect.equal
                (ValidationPackageMetadata.getSemanticVersionString Metadata.allFields)
                "1.0.0-alpha.1+build.1"
                "Semantic-version string should match"

        testCase "metadata semantic-version helpers reject invalid components" <| fun () ->
            let invalid = copyMetadata Metadata.mandatory
            invalid.MajorVersion <- -1
            Expect.isNone (ValidationPackageMetadata.tryGetSemanticVersion invalid) "Negative major should fail"

        testCase "identity contains name and complete semantic version" <| fun () ->
            let actual = ValidationPackageMetadata.getIdentity Metadata.allFields
            Expect.equal actual.Name "name" "Identity name"

            Expect.equal
                actual.Version
                SemanticVersions.prereleaseAndBuildMetadata
                "Identity version"

        testCase "identity ignores descriptive metadata" <| fun () ->
            let first = copyMetadata Metadata.allFields
            let second = copyMetadata Metadata.allFields
            second.Summary <- "Changed summary"
            second.Description <- "Changed description"
            second.Authors <- [| Author.create("Another Author") |]
            second.Inputs <- Array.empty

            Expect.isTrue
                (ValidationPackageMetadata.identityEquals(first, second))
                "Descriptive fields should not affect identity"

        testCase "identity includes name and all semantic-version fields" <| fun () ->
            let baseline = copyMetadata Metadata.allFields

            let changedName = copyMetadata Metadata.allFields
            changedName.Name <- "other"

            let changedPrerelease = copyMetadata Metadata.allFields
            changedPrerelease.PreReleaseVersionSuffix <- "rc.1"

            let changedBuild = copyMetadata Metadata.allFields
            changedBuild.BuildMetadataVersionSuffix <- "other"

            Expect.isFalse
                (ValidationPackageMetadata.identityEquals(baseline, changedName))
                "Name should affect identity"

            Expect.isFalse
                (ValidationPackageMetadata.identityEquals(baseline, changedPrerelease))
                "Prerelease should affect identity"

            Expect.isFalse
                (ValidationPackageMetadata.identityEquals(baseline, changedBuild))
                "Build metadata should affect identity"

        testCase "invalid metadata does not produce an identity" <| fun () ->
            let first = copyMetadata Metadata.mandatory
            let second = copyMetadata Metadata.mandatory
            first.MajorVersion <- -1
            second.MajorVersion <- -1

            Expect.isNone (ValidationPackageMetadata.tryGetIdentity first) "Invalid metadata identity"

            Expect.isFalse
                (ValidationPackageMetadata.identityEquals(first, second))
                "Two invalid values must not compare as the same identity"
    ]
