module AVPR.Staging.Tests.StagingRepositoryTests

open System
open System.IO
open AVPR.Staging
open Xunit

[<Fact>]
let ``repository discovery returns FSharp then Python packages`` () =
    let packages =
        StagingRepository.discover(AppContext.BaseDirectory)

    Assert.Equal(2, packages.Length)
    Assert.Equal("FSharp", packages[0].Metadata.ProgrammingLanguage)
    Assert.Equal("Python", packages[1].Metadata.ProgrammingLanguage)
    Assert.All(
        packages,
        fun stagedPackage ->
            Assert.DoesNotContain("\\", stagedPackage.RepoPath)
            Assert.Equal(0L, stagedPackage.LastUpdated.Ticks % TimeSpan.TicksPerSecond)
    )

[<Fact>]
let ``repository discovery resolves the staging area below an explicit root`` () =
    let packages =
        StagingRepository.discover(AppContext.BaseDirectory)

    Assert.All(
        packages,
        fun stagedPackage ->
            Assert.StartsWith(
                Path.Combine(AppContext.BaseDirectory, StagingConstants.RelativePath)
                    .Replace(Path.DirectorySeparatorChar, '/'),
                stagedPackage.RepoPath
            )
    )
