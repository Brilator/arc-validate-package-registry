# ValidationPackage.Codecs

Portable, pure YAML frontmatter and JSON codecs for `ValidationPackage.Model`.

The package supports .NET, Fable JavaScript, and Fable Python. It deliberately
contains no filesystem access, script discovery, package execution, or hashing.

- `ValidationPackageYaml` uses YAMLicious for both decoding and writing the
  supported validation-package shape. String scalars are explicitly
  double-quoted before YAMLicious writes the syntax tree.
- `ValidationPackageJson` defines its model codecs with Thoth.Json.Core. A
  portable JSON value runtime keeps the public string API identical on .NET,
  JavaScript, and Python without target-specific JSON dependencies.
- `Frontmatter` extracts the existing F# comment/binding and Python
  docstring/binding forms from in-memory source strings only.

JSON and YAML implementations are organized independently under `Json/` and
`Yaml/`, with separate `Encoders/` and `Decoders/` source trees. The existing
public modules remain thin façades over those implementations.

Python consumers need a `fable-library` runtime compatible with their Fable
compiler. This repository pins Fable and `fable-library` to 5.11.0 through the
dotnet tool manifest and `uv` for cross-target contract and packed-package
tests.
