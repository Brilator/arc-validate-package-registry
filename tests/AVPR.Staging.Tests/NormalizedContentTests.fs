module AVPR.Staging.Tests.NormalizedContentTests

open System.Text
open AVPR.Staging
open Xunit

[<Fact>]
let ``content normalization uses LF and UTF-8`` () =
    let actual =
        "first\r\nsecond\rthird\n"
        |> NormalizedContent.fromString
        |> Encoding.UTF8.GetString

    Assert.Equal("first\nsecond\nthird\n", actual)

[<Fact>]
let ``content hashes preserve uppercase MD5 contract across line endings`` () =
    let lf = ContentHash.ofString "first\nsecond\n"
    let crlf = ContentHash.ofString "first\r\nsecond\r\n"

    Assert.Equal(lf, crlf)
    Assert.Matches("^[0-9A-F]{32}$", lf)

[<Fact>]
let ``fixture hash matches published hash contract`` () =
    let actual =
        "fixtures/Frontmatter/Comment/valid@1.0.0.fsx"
        |> ContentHash.ofFile

    Assert.Equal("2A29D85A29D908C7DE214D56119DE207", actual)
