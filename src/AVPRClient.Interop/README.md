# AVPRClient.Interop

`AVPRClient.Interop` maps between the generated `AVPRClient` transport types
and the portable types in `ValidationPackage.Model`.

The generated client deliberately has no dependency on the portable model,
YAML codecs, or AVPR staging infrastructure. Applications that need model
conversion can reference this package explicitly and import
`AVPRClient.Interop` to use the `ToModel`, `ToClient`, and `IdentityEquals`
extension methods.

Converting portable metadata to a client validation package requires callers
to provide the package bytes and release date because those transport fields
are not part of the portable metadata model.
