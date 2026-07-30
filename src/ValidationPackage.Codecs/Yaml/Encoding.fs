namespace ValidationPackage.Codecs.Yaml

open YAMLicious
open YAMLicious.YAMLiciousTypes

[<RequireQualifiedAccess>]
module internal Encoding =

    let string value =
        YAMLContent.create(value, style = ScalarStyle.DoubleQuoted)
        |> YAMLElement.Value

    let int value = Encode.int value
    let bool value = Encode.bool value
    let object values = Encode.object values

    let array values =
        values
        |> Array.toList
        |> YAMLElement.Sequence

    let write value = Encode.write 2 value
