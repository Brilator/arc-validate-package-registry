namespace ValidationPackage.Codecs.Yaml.Decoders

open System
open YAMLicious
open YAMLicious.YAMLiciousTypes
open ValidationPackage.Model
open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal Cwl =

    let commandInputType element =
        let value =
            match element with
            | YAMLElement.Value _
            | YAMLElement.Object [ YAMLElement.Value _ ] ->
                Decode.string element
            | _ ->
                invalidArg
                    "element"
                    "CWL command input type must be one supported scalar type string"

        try
            CommandInputType.fromCwlString value
        with
        | :? ArgumentException ->
            invalidArg "element" $"unsupported CWL command input type: {value}"

    let commandInputBinding element =
        match element with
        | YAMLElement.Object _ ->
            Decode.object (fun get ->
                CommandInputBinding.create(
                    Position = (
                        get.Optional.Field "position" Decode.int
                        |> Option.defaultValue 0
                    ),
                    Prefix = (
                        get.Optional.Field "prefix" Decode.string
                        |> Option.defaultValue ""
                    ),
                    Separate = (
                        get.Optional.Field "separate" Decode.bool
                        |> Option.defaultValue true
                    )
                )
            ) element
        | _ ->
            invalidArg
                "element"
                "CWL command input inputBinding must be a mapping"

    let commandInputParameter element =
        match element with
        | YAMLElement.Object _ ->
            Decode.object (fun get ->
                let id = get.Optional.Field "id" Decode.string
                let inputType = get.Optional.Field "type" commandInputType

                let inputBinding =
                    get.Optional.Field "inputBinding" commandInputBinding

                match id, inputType, inputBinding with
                | Some id, Some inputType, Some inputBinding ->
                    CommandInputParameter.create(
                        id,
                        inputType,
                        inputBinding,
                        Label = (
                            get.Optional.Field "label" Decode.string
                            |> Option.defaultValue ""
                        ),
                        Doc = (
                            get.Optional.Field "doc" Decode.string
                            |> Option.defaultValue ""
                        )
                    )
                | _ ->
                    let missingFields = [
                        if id.IsNone then
                            "id"

                        if inputType.IsNone then
                            "type"

                        if inputBinding.IsNone then
                            "inputBinding"
                    ]

                    let missingFieldNames = String.concat ", " missingFields

                    invalidArg
                        "element"
                        $"CWL command input parameter is missing required field(s): {missingFieldNames}"
            ) element
        | _ ->
            invalidArg
                "element"
                "each CWL command input parameter must be a mapping"

    let commandInputParameters element =
        match element with
        | YAMLElement.Sequence _
        | YAMLElement.Object [ YAMLElement.Sequence _ ] ->
            element
            |> Decode.array commandInputParameter
            |> CwlValidation.validateParameters
        | _ ->
            invalidArg "element" "AVPR Inputs must use the CWL array form"
