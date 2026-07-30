namespace ValidationPackage.Codecs.Json.Encoders

open Thoth.Json.Core

[<RequireQualifiedAccess>]
module internal Author =

    let encode (author: ValidationPackage.Model.Author) =
        Encode.object [
            "fullName", Encode.string author.FullName
            "email", Encode.string author.Email
            "affiliation", Encode.string author.Affiliation
            "affiliationLink", Encode.string author.AffiliationLink
        ]
