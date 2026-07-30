namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module CwlYaml =

    let internal commandInputTypeEncoder = Yaml.Encoders.Cwl.commandInputType
    let commandInputTypeDecoder = Yaml.Decoders.Cwl.commandInputType
    let internal commandInputBindingEncoder = Yaml.Encoders.Cwl.commandInputBinding
    let commandInputBindingDecoder = Yaml.Decoders.Cwl.commandInputBinding
    let internal commandInputParameterEncoder = Yaml.Encoders.Cwl.commandInputParameter
    let commandInputParameterDecoder = Yaml.Decoders.Cwl.commandInputParameter
    let validateParameters = Yaml.CwlValidation.validateParameters
    let internal commandInputParametersEncoder = Yaml.Encoders.Cwl.commandInputParameters
    let commandInputParametersDecoder = Yaml.Decoders.Cwl.commandInputParameters
