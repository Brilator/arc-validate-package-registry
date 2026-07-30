namespace ValidationPackage.Codecs.Yaml.Encoders

open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal Author =

    let encode (author: ValidationPackage.Model.Author) =
        Encoding.object [
            "FullName", Encoding.string author.FullName
            "Email", Encoding.string author.Email
            "Affiliation", Encoding.string author.Affiliation
            "AffiliationLink", Encoding.string author.AffiliationLink
        ]
