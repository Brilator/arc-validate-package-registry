namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module CwlJson =

    let commandInputTypeEncoder = Json.Encoders.Cwl.commandInputType
    let commandInputTypeDecoder = Json.Decoders.Cwl.commandInputType
    let commandInputBindingEncoder = Json.Encoders.Cwl.commandInputBinding
    let commandInputBindingDecoder = Json.Decoders.Cwl.commandInputBinding
    let commandInputParameterEncoder = Json.Encoders.Cwl.commandInputParameter
    let commandInputParameterDecoder = Json.Decoders.Cwl.commandInputParameter
