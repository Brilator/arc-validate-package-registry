namespace ValidationPackage.Model

open Fable.Core

[<AttachMembers>]
type ValidationPackageIdentity(name: string, version: SemVer) =

    member _.Name = name
    member _.Version = version

    override this.GetHashCode() =
        ValidationPackageIdentity.getHashCode(this)

    static member getHashCode(identity: ValidationPackageIdentity) =
        PortableHash.combineValues [
            PortableHash.stringValue identity.Name
            SemVer.getHashCode identity.Version
        ]

    override this.Equals(other) =
        match other with
        | :? ValidationPackageIdentity as identity ->
            (this.Name, this.Version) = (identity.Name, identity.Version)
        | _ -> false

    static member create(name: string, version: SemVer) =
        ValidationPackageIdentity(name, version)
