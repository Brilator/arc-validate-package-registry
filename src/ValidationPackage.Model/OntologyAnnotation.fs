namespace ValidationPackage.Model

open Fable.Core

[<AttachMembers>]
type OntologyAnnotation() =

    let mutable _name = ""
    let mutable _termSourceREF = ""
    let mutable _termAccessionNumber = ""

    member _.Name
        with get () = _name
        and set value = _name <- value

    member _.TermSourceREF
        with get () = _termSourceREF
        and set value = _termSourceREF <- value

    member _.TermAccessionNumber
        with get () = _termAccessionNumber
        and set value = _termAccessionNumber <- value

    override this.GetHashCode() =
        OntologyAnnotation.getHashCode(this)

    static member getHashCode(annotation: OntologyAnnotation) =
        PortableHash.combineValues [
            PortableHash.stringValue annotation.Name
            PortableHash.stringValue annotation.TermSourceREF
            PortableHash.stringValue annotation.TermAccessionNumber
        ]

    override this.Equals(other) =
        match other with
        | :? OntologyAnnotation as annotation ->
            (
                this.Name,
                this.TermSourceREF,
                this.TermAccessionNumber
            ) = (
                annotation.Name,
                annotation.TermSourceREF,
                annotation.TermAccessionNumber
            )
        | _ -> false

    static member create (
        name: string,
        ?TermSourceRef: string,
        ?TermAccessionNumber: string
    ) =
        let annotation = OntologyAnnotation(Name = name)
        TermSourceRef |> Option.iter (fun value -> annotation.TermSourceREF <- value)
        TermAccessionNumber |> Option.iter (fun value -> annotation.TermAccessionNumber <- value)
        annotation
