module Domain

open System
open System.Text.Json

let jsonSerializerOptions = JsonSerializerOptions(WriteIndented = true)

module RegistryEndpoint =

    let normalize (baseUrl: string) =
        match Uri.TryCreate(baseUrl, UriKind.Absolute) with
        | true, uri
            when (uri.Scheme = Uri.UriSchemeHttp || uri.Scheme = Uri.UriSchemeHttps)
                 && String.IsNullOrEmpty(uri.Query)
                 && String.IsNullOrEmpty(uri.Fragment) ->
            uri.AbsoluteUri.TrimEnd('/')
        | _ ->
            invalidArg
                (nameof baseUrl)
                "The registry base URL must be an absolute HTTP(S) URL without a query string or fragment."

type AVPRClient.ValidationPackage with
    
    static member toJson (p: AVPRClient.ValidationPackage) = 
        JsonSerializer.Serialize(p, jsonSerializerOptions)

    static member printJson (p: AVPRClient.ValidationPackage) = 
        let json = AVPRClient.ValidationPackage.toJson p
        printfn ""
        printfn $"Package info:{System.Environment.NewLine}{json}"
        printfn ""
