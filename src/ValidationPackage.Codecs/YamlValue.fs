namespace ValidationPackage.Codecs

open System

type internal YamlValue =
    | StringValue of string
    | IntValue of int
    | BoolValue of bool
    | ObjectValue of (string * YamlValue) list
    | ArrayValue of YamlValue list

[<RequireQualifiedAccess>]
module internal YamlValue =

    let string value = StringValue value
    let int value = IntValue value
    let bool value = BoolValue value
    let object values = ObjectValue values
    let array values = values |> Array.toList |> ArrayValue

    let private escapeString (value: string) =
        value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t")

    let private scalarText value =
        match value with
        | StringValue value -> $"\"{escapeString value}\""
        | IntValue value -> value.ToString()
        | BoolValue value -> if value then "true" else "false"
        | ObjectValue _
        | ArrayValue _ -> invalidArg "value" "Expected a scalar YAML value"

    let private spaces count = String(' ', count)

    let rec private writeValue indent value =
        match value with
        | StringValue _
        | IntValue _
        | BoolValue _ -> scalarText value
        | ObjectValue [] -> "{}"
        | ArrayValue [] -> "[]"
        | ObjectValue fields -> writeObject indent fields
        | ArrayValue values -> writeArray indent values

    and private writeObject indent fields =
        fields
        |> List.map (fun (key, value) ->
            let prefix = spaces indent + key + ":"

            match value with
            | StringValue _
            | IntValue _
            | BoolValue _ -> prefix + " " + scalarText value
            | ObjectValue [] -> prefix + " {}"
            | ArrayValue [] -> prefix + " []"
            | ObjectValue _
            | ArrayValue _ -> prefix + "\n" + writeValue (indent + 2) value
        )
        |> String.concat "\n"

    and private writeArray indent values =
        values
        |> List.map (fun value ->
            let prefix = spaces indent + "-"

            match value with
            | StringValue _
            | IntValue _
            | BoolValue _ -> prefix + " " + scalarText value
            | ObjectValue [] -> prefix + " {}"
            | ArrayValue [] -> prefix + " []"
            | ObjectValue _
            | ArrayValue _ -> prefix + "\n" + writeValue (indent + 2) value
        )
        |> String.concat "\n"

    let write value = writeValue 0 value + "\n"
