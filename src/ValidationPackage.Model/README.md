# ValidationPackage.Model

Portable validation-package domain types for .NET and Fable targets.

The package contains metadata, authors, ontology tags, the supported CWL
command-input subset, semantic versions, and package identity. It intentionally
contains no YAML, JSON, filesystem, hashing, HTTP, EF, OpenAPI, or AVPR staging
logic.

String codecs are intentionally out of scope here and will be provided
separately by `ValidationPackage.Codecs`.

The first codec implementation should target `ValidationPackage.Model` 0.1.0.
If the model contract changes before that handoff, version both sides
deliberately rather than relying on an unconstrained package range.
