namespace ValidationPackage.Model

open System
open Fable.Core

type CwlPrimitive =
    | Boolean = 0
    | Int = 1
    | Long = 2
    | Float = 3
    | Double = 4
    | String = 5

[<AttachMembers>]
type CommandInputType() =

    let mutable _primitiveType = CwlPrimitive.String
    let mutable _isNullable = false

    member _.PrimitiveType
        with get () = _primitiveType
        and set value =
            match value with
            | CwlPrimitive.Boolean
            | CwlPrimitive.Int
            | CwlPrimitive.Long
            | CwlPrimitive.Float
            | CwlPrimitive.Double
            | CwlPrimitive.String ->
                _primitiveType <- value
            | _ ->
                invalidArg "PrimitiveType" $"Invalid primitive type: {value}"

    member _.IsNullable
        with get () = _isNullable
        and set value = _isNullable <- value

    override this.GetHashCode() =
        CommandInputType.getHashCode(this)

    static member getHashCode(inputType: CommandInputType) =
        PortableHash.combineValues [
            int inputType.PrimitiveType
            PortableHash.boolValue inputType.IsNullable
        ]

    override this.Equals(other) =
        match other with
        | :? CommandInputType as inputType ->
            (this.PrimitiveType, this.IsNullable) =
                (inputType.PrimitiveType, inputType.IsNullable)
        | _ -> false

    static member create (
        primitiveType: CwlPrimitive,
        ?IsNullable: bool
    ) =
        let inputType = CommandInputType(PrimitiveType = primitiveType)
        IsNullable |> Option.iter (fun value -> inputType.IsNullable <- value)
        inputType

    static member fromCwlString(value: string) =
        if isNull value then
            nullArg "value"

        let isNullable = value.EndsWith("?", StringComparison.Ordinal)

        let primitiveName =
            if isNullable then
                value.Substring(0, value.Length - 1)
            else
                value

        let primitiveType =
            match primitiveName with
            | "boolean" -> CwlPrimitive.Boolean
            | "int" -> CwlPrimitive.Int
            | "long" -> CwlPrimitive.Long
            | "float" -> CwlPrimitive.Float
            | "double" -> CwlPrimitive.Double
            | "string" -> CwlPrimitive.String
            | _ -> invalidArg "value" $"unsupported CWL command input type: {value}"

        CommandInputType.create(primitiveType, isNullable)

    static member toCwlString(inputType: CommandInputType) =
        if isNull (box inputType) then
            nullArg "inputType"

        let primitiveName =
            match inputType.PrimitiveType with
            | CwlPrimitive.Boolean -> "boolean"
            | CwlPrimitive.Int -> "int"
            | CwlPrimitive.Long -> "long"
            | CwlPrimitive.Float -> "float"
            | CwlPrimitive.Double -> "double"
            | CwlPrimitive.String -> "string"
            | value -> invalidArg "inputType" $"unsupported CWL primitive type: {value}"

        if inputType.IsNullable then
            $"{primitiveName}?"
        else
            primitiveName

[<AttachMembers>]
type CommandInputBinding() =

    let mutable _position = 0
    let mutable _prefix = ""
    let mutable _separate = true

    member _.Position
        with get () = _position
        and set value = _position <- value

    member _.Prefix
        with get () = _prefix
        and set value = _prefix <- value

    member _.Separate
        with get () = _separate
        and set value = _separate <- value

    override this.GetHashCode() =
        CommandInputBinding.getHashCode(this)

    static member getHashCode(binding: CommandInputBinding) =
        PortableHash.combineValues [
            binding.Position
            PortableHash.stringValue binding.Prefix
            PortableHash.boolValue binding.Separate
        ]

    override this.Equals(other) =
        match other with
        | :? CommandInputBinding as binding ->
            (this.Position, this.Prefix, this.Separate) =
                (binding.Position, binding.Prefix, binding.Separate)
        | _ -> false

    static member create (
        ?Position: int,
        ?Prefix: string,
        ?Separate: bool
    ) =
        let binding = CommandInputBinding()
        Position |> Option.iter (fun value -> binding.Position <- value)
        Prefix |> Option.iter (fun value -> binding.Prefix <- value)
        Separate |> Option.iter (fun value -> binding.Separate <- value)
        binding

[<AttachMembers>]
type CommandInputParameter() =

    let mutable _id = ""
    let mutable _type = CommandInputType.create(CwlPrimitive.String)
    let mutable _label = ""
    let mutable _doc = ""
    let mutable _inputBinding = CommandInputBinding()

    member _.Id
        with get () = _id
        and set value = _id <- value

    member _.Type
        with get () = _type
        and set value = _type <- value

    member _.Label
        with get () = _label
        and set value = _label <- value

    member _.Doc
        with get () = _doc
        and set value = _doc <- value

    member _.InputBinding
        with get () = _inputBinding
        and set value = _inputBinding <- value

    override this.GetHashCode() =
        CommandInputParameter.getHashCode(this)

    static member getHashCode(parameter: CommandInputParameter) =
        PortableHash.combineValues [
            PortableHash.stringValue parameter.Id
            CommandInputType.getHashCode parameter.Type
            PortableHash.stringValue parameter.Label
            PortableHash.stringValue parameter.Doc
            CommandInputBinding.getHashCode parameter.InputBinding
        ]

    override this.Equals(other) =
        match other with
        | :? CommandInputParameter as parameter ->
            (
                this.Id,
                this.Type,
                this.Label,
                this.Doc,
                this.InputBinding
            ) = (
                parameter.Id,
                parameter.Type,
                parameter.Label,
                parameter.Doc,
                parameter.InputBinding
            )
        | _ -> false

    static member create (
        id: string,
        inputType: CommandInputType,
        inputBinding: CommandInputBinding,
        ?Label: string,
        ?Doc: string
    ) =
        let parameter =
            CommandInputParameter(
                Id = id,
                Type = inputType,
                InputBinding = inputBinding
            )

        Label |> Option.iter (fun value -> parameter.Label <- value)
        Doc |> Option.iter (fun value -> parameter.Doc <- value)
        parameter
