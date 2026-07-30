module ValidationPackage.Codecs.Tests.FrontmatterTests

open Fable.Pyxpecto
open ValidationPackage.Codecs
open ValidationPackage.Codecs.Tests.ReferenceObjects

let tests =
    testList "frontmatter boundaries" [
        testCase "language aliases are preserved" <| fun () ->
            Expect.equal
                (FrontmatterLanguage.fromString "f#")
                FrontmatterLanguage.FSharp
                "F# alias"

            Expect.equal
                (FrontmatterLanguage.fromString "py")
                FrontmatterLanguage.Python
                "Python alias"

            Expect.equal
                (FrontmatterLanguage.toString FrontmatterLanguage.FSharp)
                "FSharp"
                "F# metadata name"

        testCase "F# comment and binding forms extract the same YAML" <| fun () ->
            Expect.equal
                (Frontmatter.extract FrontmatterLanguage.FSharp fsharpComment)
                yaml
                "F# comment"

            Expect.equal
                (Frontmatter.extract FrontmatterLanguage.FSharp fsharpBinding)
                yaml
                "F# binding"

        testCase "Python comment and binding forms extract the same YAML" <| fun () ->
            Expect.equal
                (Frontmatter.extract FrontmatterLanguage.Python pythonComment)
                yaml
                "Python comment"

            Expect.equal
                (Frontmatter.extract FrontmatterLanguage.Python pythonBinding)
                yaml
                "Python binding"

        testCase "Windows line endings are normalized" <| fun () ->
            let windowsSource = fsharpComment.Replace("\n", "\r\n")

            Expect.equal
                (Frontmatter.extract FrontmatterLanguage.FSharp windowsSource)
                yaml
                "CRLF source"

        testCase "invalid boundaries return no frontmatter" <| fun () ->
            Expect.isNone
                (Frontmatter.tryExtract FrontmatterLanguage.FSharp "printfn \"none\"")
                "Invalid F# source"

            Expect.isNone
                (Frontmatter.tryExtract FrontmatterLanguage.Python "print('none')")
                "Invalid Python source"
    ]
