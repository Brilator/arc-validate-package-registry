open ValidationPackage.Codecs
open ValidationPackage.Model

let metadata =
    ValidationPackageMetadata.create(
        name = "codec-smoke",
        summary = "Packed codec smoke test",
        description = "Verifies consumption without project references.",
        majorVersion = 1,
        minorVersion = 0,
        patchVersion = 0,
        programmingLanguage = "FSharp"
    )

let json = ValidationPackageJson.encode metadata
let fromJson = ValidationPackageJson.decodeOrFail json
let yaml = ValidationPackageYaml.encode metadata
let fromYaml = ValidationPackageYaml.decodeOrFail yaml

if fromJson <> metadata then
    failwith "Packed codec JSON round-trip changed the metadata."

if fromYaml <> metadata then
    failwith "Packed codec YAML round-trip changed the metadata."

printfn "ValidationPackage.Codecs packed-package smoke test passed."
