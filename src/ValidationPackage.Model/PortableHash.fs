namespace ValidationPackage.Model

module internal PortableHash =

    let combine (current: int) (next: int) =
        (current * 397) ^^^ next

    let combineValues values =
        values |> Seq.fold combine 17

    let stringValue (value: string) =
        if isNull value then
            0
        else
            value
            |> Seq.fold (fun current character -> combine current (int character)) 17

    let boolValue value =
        if value then 1 else 0

    let arrayValue projection values =
        values
        |> Array.fold (fun current value -> combine current (projection value)) 17
