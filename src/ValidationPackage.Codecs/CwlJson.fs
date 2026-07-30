namespace ValidationPackage.Codecs

open System
open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module CwlJson =

    let commandInputTypeEncoder (inputType: CommandInputType) =
        inputType
        |> CommandInputType.toCwlString
        |> Encode.string

    let commandInputTypeDecoder: Decoder<CommandInputType> =
        Decode.string
        |> Decode.andThen (fun value ->
            try
                value
                |> CommandInputType.fromCwlString
                |> Decode.succeed
            with error ->
                Decode.fail error.Message
        )

    let commandInputBindingEncoder (binding: CommandInputBinding) =
        Encode.object [
            "position", Encode.int binding.Position
            "prefix", Encode.string binding.Prefix
            "separate", Encode.bool binding.Separate
        ]

    let commandInputBindingDecoder: Decoder<CommandInputBinding> =
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
        )

    let commandInputParameterEncoder (parameter: CommandInputParameter) =
        Encode.object [
            "id", Encode.string parameter.Id
            "type", commandInputTypeEncoder parameter.Type
            "label", Encode.string parameter.Label
            "doc", Encode.string parameter.Doc
            "inputBinding", commandInputBindingEncoder parameter.InputBinding
        ]

    let commandInputParameterDecoder: Decoder<CommandInputParameter> =
        Decode.object (fun get ->
            CommandInputParameter.create(
                get.Required.Field "id" Decode.string,
                get.Required.Field "type" commandInputTypeDecoder,
                get.Required.Field "inputBinding" commandInputBindingDecoder,
                Label = (
                    get.Optional.Field "label" Decode.string
                    |> Option.defaultValue ""
                ),
                Doc = (
                    get.Optional.Field "doc" Decode.string
                    |> Option.defaultValue ""
                )
            )
        )
