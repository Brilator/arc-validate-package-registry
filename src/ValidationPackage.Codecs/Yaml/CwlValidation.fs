namespace ValidationPackage.Codecs.Yaml

open System
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal CwlValidation =

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
