namespace ValidationPackage.Codecs

open System
open YAMLicious
open YAMLicious.YAMLiciousTypes
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module CwlYaml =

    let internal commandInputTypeEncoder (inputType: CommandInputType) =
        inputType
        |> CommandInputType.toCwlString
        |> YamlValue.string

    let commandInputTypeDecoder element =
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

    let internal commandInputBindingEncoder (binding: CommandInputBinding) =
        [
            if binding.Position <> 0 then
                "position", YamlValue.int binding.Position

            if binding.Prefix <> "" then
                "prefix", YamlValue.string binding.Prefix

            if not binding.Separate then
                "separate", YamlValue.bool binding.Separate
        ]
        |> YamlValue.object

    let commandInputBindingDecoder element =
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

    let internal commandInputParameterEncoder (parameter: CommandInputParameter) =
        [
                "id", YamlValue.string parameter.Id
                "type", commandInputTypeEncoder parameter.Type

                if parameter.Label <> "" then
                    "label", YamlValue.string parameter.Label

                if parameter.Doc <> "" then
                    "doc", YamlValue.string parameter.Doc

                "inputBinding", commandInputBindingEncoder parameter.InputBinding
        ]
        |> YamlValue.object

    let commandInputParameterDecoder element =
        match element with
        | YAMLElement.Object _ ->
            Decode.object (fun get ->
                let id = get.Optional.Field "id" Decode.string
                let inputType = get.Optional.Field "type" commandInputTypeDecoder

                let inputBinding =
                    get.Optional.Field
                        "inputBinding"
                        commandInputBindingDecoder

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

    let validateParameters (parameters: CommandInputParameter array) =
        parameters
        |> Array.iteri (fun index parameter ->
            if String.IsNullOrWhiteSpace(parameter.Id) then
                invalidArg
                    "parameters"
                    $"CWL command input parameter at index {index} requires a non-empty id"
        )

        parameters
        |> Array.countBy (fun parameter -> parameter.Id)
        |> Array.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun (id, _) ->
            invalidArg
                "parameters"
                $"CWL command input parameter id must be unique, but was duplicated: {id}"
        )

        parameters

    let internal commandInputParametersEncoder parameters =
        parameters
        |> validateParameters
        |> Array.map commandInputParameterEncoder
        |> YamlValue.array

    let commandInputParametersDecoder element =
        match element with
        | YAMLElement.Sequence _
        | YAMLElement.Object [ YAMLElement.Sequence _ ] ->
            element
            |> Decode.array commandInputParameterDecoder
            |> validateParameters
        | _ ->
            invalidArg "element" "AVPR Inputs must use the CWL array form"
