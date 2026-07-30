# ValidationPackage.Codecs

Portable, pure YAML frontmatter and JSON codecs for `ValidationPackage.Model`.

The package supports .NET, Fable JavaScript, and Fable Python. It deliberately
contains no filesystem access, script discovery, package execution, or hashing.

- `ValidationPackageYaml` decodes YAML with YAMLicious and emits the supported
  validation-package shape with a small portable writer. The local writer is a
  temporary compatibility boundary for the open YAMLicious/Fable Python writer
  defect tracked in [YAMLicious #20](https://github.com/CSBiology/YAMLicious/issues/20).
- `ValidationPackageJson` defines its model codecs with Thoth.Json.Core. A
  portable JSON value runtime keeps the public string API identical on .NET,
  JavaScript, and Python without target-specific JSON dependencies.
- `Frontmatter` extracts the existing F# comment/binding and Python
  docstring/binding forms from in-memory source strings only.

Python consumers need the matching `fable-library` runtime. This repository
pins it through `uv` for the cross-target contract and packed-package tests.
