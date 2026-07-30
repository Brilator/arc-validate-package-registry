module ValidationPackage.Codecs.Tests.JsonTests

open Fable.Pyxpecto
open ValidationPackage.Codecs
open ValidationPackage.Codecs.Tests.ReferenceObjects

let private expectError message result =
    match result with
    | Ok _ -> failwith message
    | Error error -> Expect.isNonEmpty error message

let tests =
    testList "JSON codecs" [
        testCase "metadata round-trips through the target runtime" <| fun () ->
            let encoded = ValidationPackageJson.encode metadata
            let actual = ValidationPackageJson.decodeOrFail encoded
            Expect.equal actual metadata "JSON round-trip"

        testCase "wire names preserve the API contract" <| fun () ->
            let encoded = ValidationPackageJson.encode metadata
            Expect.stringContains encoded "\"name\":\"test-package\"" "camelCase metadata"
            Expect.stringContains encoded "\"Inputs\":[" "PascalCase Inputs wrapper"
            Expect.stringContains encoded "\"id\":\"output\"" "lower-camel id"
            Expect.stringContains encoded "\"type\":\"string\"" "scalar CWL type"
            Expect.stringContains encoded "\"inputBinding\":" "lower-camel binding"
            Expect.stringContains encoded "\"termSourceREF\":\"AVPR\"" "ontology acronym"

        testCase "missing metadata fields retain defaults" <| fun () ->
            let actual = ValidationPackageJson.decodeOrFail "{}"
            Expect.equal actual.Name "" "Name default"
            Expect.equal actual.MajorVersion -1 "Major version default"
            Expect.isEmpty actual.Authors "Authors default"
            Expect.isEmpty actual.Inputs "Inputs default"

        testCase "unknown JSON fields are ignored" <| fun () ->
            let actual =
                ValidationPackageJson.decodeOrFail
                    """{"name":"compatible","future":{"nested":true}}"""

            Expect.equal actual.Name "compatible" "Known field"

        testCase "CWL binding fields use documented defaults" <| fun () ->
            let actual =
                ValidationPackageJson.decodeOrFail
                    """{"Inputs":[{"id":"value","type":"string?","inputBinding":{}}]}"""

            let input = actual.Inputs[0]
            Expect.isTrue input.Type.IsNullable "Nullable scalar"
            Expect.equal input.InputBinding.Position 0 "Position default"
            Expect.equal input.InputBinding.Prefix "" "Prefix default"
            Expect.isTrue input.InputBinding.Separate "Separate default"

        testCase "required CWL JSON fields fail when absent" <| fun () ->
            ValidationPackageJson.decode
                """{"Inputs":[{"id":"value","type":"string"}]}"""
            |> expectError "Missing inputBinding should fail"

        testCase "unsupported CWL JSON strings and shapes fail" <| fun () ->
            ValidationPackageJson.decode
                """{"Inputs":[{"id":"file","type":"File","inputBinding":{}}]}"""
            |> expectError "Unsupported scalar should fail"

            ValidationPackageJson.decode
                """{"Inputs":[{"id":"value","type":{"primitive":"string"},"inputBinding":{}}]}"""
            |> expectError "Object storage shape should not leak"
    ]
