module AVPR.Staging.Tests.StagedValidationPackageTests

open System
open AVPR.Staging
open Xunit

let private testDate =
    DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(1.0))

[<Fact>]
let ``FSharp staged package preserves metadata path and hash`` () =
    let actual =
        StagedValidationPackage.fromFile
            "fixtures/Frontmatter/Comment/valid@1.0.0.fsx"
            testDate

    Assert.Equal("valid", actual.Metadata.Name)
    Assert.Equal("FSharp", actual.Metadata.ProgrammingLanguage)
    Assert.Equal("valid@1.0.0.fsx", actual.FileName)
    Assert.Equal(testDate, actual.LastUpdated)
    Assert.Equal("4CD3A22514059AC2E49BCD213FA7D751", actual.ContentHash)
    Assert.Equal("1.0.0", StagedValidationPackage.getSemanticVersionString actual)

[<Fact>]
let ``Python staged package preserves metadata path and hash`` () =
    let actual =
        StagedValidationPackage.fromFile
            "fixtures/Frontmatter/Comment/valid@1.0.0.py"
            testDate

    Assert.Equal("valid", actual.Metadata.Name)
    Assert.Equal("Python", actual.Metadata.ProgrammingLanguage)
    Assert.Equal("valid@1.0.0.py", actual.FileName)
    Assert.Equal("75CEB39AF4DEAD74685E44DCD72DC85E", actual.ContentHash)

[<Fact>]
let ``staged identity and content comparisons are independent`` () =
    let first =
        StagedValidationPackage.fromFile
            "fixtures/Frontmatter/Comment/valid@1.0.0.fsx"
            testDate

    let sameIdentity =
        { first with ContentHash = "different" }

    Assert.True(StagedValidationPackage.identityEquals first sameIdentity)
    Assert.False(StagedValidationPackage.contentEquals first sameIdentity)

[<Fact>]
let ``unsupported script extensions fail before parsing`` () =
    let error =
        Assert.Throws<ArgumentException>(fun () ->
            StagedValidationPackage.fromFile "package.txt" testDate
            |> ignore
        )

    Assert.Contains("Unsupported validation-package script extension", error.Message)
