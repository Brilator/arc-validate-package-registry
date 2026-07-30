namespace ValidationPackage.Model

open Fable.Core

[<AttachMembers>]
type Author() =

    let mutable _fullName = ""
    let mutable _email = ""
    let mutable _affiliation = ""
    let mutable _affiliationLink = ""

    member _.FullName
        with get () = _fullName
        and set value = _fullName <- value

    member _.Email
        with get () = _email
        and set value = _email <- value

    member _.Affiliation
        with get () = _affiliation
        and set value = _affiliation <- value

    member _.AffiliationLink
        with get () = _affiliationLink
        and set value = _affiliationLink <- value

    override this.GetHashCode() =
        Author.getHashCode(this)

    static member getHashCode(author: Author) =
        PortableHash.combineValues [
            PortableHash.stringValue author.FullName
            PortableHash.stringValue author.Email
            PortableHash.stringValue author.Affiliation
            PortableHash.stringValue author.AffiliationLink
        ]

    override this.Equals(other) =
        match other with
        | :? Author as author ->
            (
                this.FullName,
                this.Email,
                this.Affiliation,
                this.AffiliationLink
            ) = (
                author.FullName,
                author.Email,
                author.Affiliation,
                author.AffiliationLink
            )
        | _ -> false

    static member create (
        fullName: string,
        ?Email: string,
        ?Affiliation: string,
        ?AffiliationLink: string
    ) =
        let author = Author(FullName = fullName)
        Email |> Option.iter (fun value -> author.Email <- value)
        Affiliation |> Option.iter (fun value -> author.Affiliation <- value)
        AffiliationLink |> Option.iter (fun value -> author.AffiliationLink <- value)
        author
