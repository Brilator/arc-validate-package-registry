open ValidationPackage.Model

let metadata =
    ValidationPackageMetadata.create(
        name = "package-smoke",
        summary = "Packed package smoke test",
        description = "Verifies consumption without a project reference.",
        majorVersion = 1,
        minorVersion = 2,
        patchVersion = 3,
        programmingLanguage = "FSharp"
    )

let identity = ValidationPackageMetadata.getIdentity metadata

if identity.Name <> "package-smoke" then
    failwith "Packed model returned the wrong package identity name."

if SemVer.toString identity.Version <> "1.2.3" then
    failwith "Packed model returned the wrong semantic version."

printfn "ValidationPackage.Model packed-package smoke test passed."
