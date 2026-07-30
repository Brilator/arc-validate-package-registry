# AVPR #111, phase 2: extract `ValidationPackage.Codecs`

## Status

Implemented delivery plan for the codec slice of
[AVPR #111](https://github.com/nfdi4plants/arc-validate-package-registry/issues/111).

This phase creates and proves the portable codec package. It is additive:
`AVPRIndex.Frontmatter` and current production consumers remain unchanged
until their owning migration issues move them to the portable boundary.

## Objective

Create a publishable `ValidationPackage.Codecs` F# library that converts
in-memory strings to and from `ValidationPackage.Model` on .NET, JavaScript,
and Python.

The package must:

- preserve the existing F# and Python frontmatter boundary forms;
- preserve PascalCase YAML metadata names;
- preserve the public JSON contract, including camel-case metadata,
  PascalCase `Inputs`, lower-camel CWL fields, and scalar CWL types;
- retain defaults, unknown-field tolerance, and actionable CWL failures;
- contain no filesystem access, repository traversal, content hashing, HTTP,
  package execution, or staging behavior;
- work from packed NuGet sources, not only through project references.

## Boundary and layout

```text
src/ValidationPackage.Codecs/
  FrontmatterLanguage.fs
  Frontmatter.fs
  YamlValue.fs
  CwlYaml.fs
  AuthorYaml.fs
  OntologyAnnotationYaml.fs
  ValidationPackageYaml.fs
  JsonRuntime.fs
  CwlJson.fs
  AuthorJson.fs
  OntologyAnnotationJson.fs
  ValidationPackageJson.fs
  targets/
    ValidationPackage.Codecs.Javascript.fsproj
    ValidationPackage.Codecs.Python.fsproj

tests/ValidationPackage.Codecs.Tests/
  ReferenceObjects.fs
  FrontmatterTests.fs
  YamlTests.fs
  JsonTests.fs
  Main.fs
  javascript/
  python/

tests/ValidationPackage.Codecs.PackageSmoke/
```

The split source layout mirrors the model: CWL codecs may stay grouped, while
author, ontology annotation, frontmatter, and top-level metadata concerns live
in separate files.

## Codec decisions

### YAML and frontmatter

- Use YAMLicious to parse YAML into its portable syntax tree and decode model
  fields.
- Keep frontmatter extraction as pure string logic with exact F#/Python
  comment and binding delimiters and newline normalization.
- Emit only the supported validation-package shape with a small portable YAML
  writer. YAMLicious's Python writer is currently unusable with Fable 5 because
  of the open generated-code defect in
  [YAMLicious #20](https://github.com/CSBiology/YAMLicious/issues/20).
- Quote emitted string scalars so metadata containing YAML punctuation,
  whitespace, or line breaks remains unambiguous.

### JSON

- Define every domain encoder and decoder with `Thoth.Json.Core`.
- Keep encoder/decoder values at module scope rather than as model static
  members, avoiding Fable Python self-reference initialization defects.
- Use a small portable `Thoth.Json.Core.Json` string runtime so the package
  does not require mutually exclusive Newtonsoft, JavaScript, and Python
  runtime dependencies.
- Reject the database-only object representation of CWL input types.

### CWL compatibility

- Decode and encode the twelve supported required/nullable scalar strings.
- Require `id`, `type`, and `inputBinding` for each parameter.
- Default binding `position` to `0`, `prefix` to `""`, and `separate` to
  `true`.
- Require `Inputs` to use the sequence form and enforce non-empty unique ids.
- Ignore unsupported extra parameter and binding fields.

## Verification sequence

1. Build and run one 22-case Pyxpecto contract suite on .NET.
2. Transpile and run the same cases with Fable JavaScript.
3. Transpile and run the same cases with Fable Python through the locked uv
   environment.
4. Pack both `ValidationPackage.Model` and `ValidationPackage.Codecs`.
5. Restore a smoke project from those local packages and run it on all three
   targets.
6. Verify the NuGet archive includes the codec project and every ordered F#
   source under `fable/`.
7. Build and test the main solution to prove the additive slice does not alter
   current registry behavior.

## Deferred work

- Replacing `AVPRIndex.Frontmatter` and moving script file reading belongs to
  AVPR staging migration work.
- Switching ARCExpect metadata setup to the new package belongs to the
  arc-validate roadmap.
- Removing the local YAML emitter should be reconsidered after YAMLicious #20
  is fixed and a compatible release is available.
- Canonical cross-repository fixtures remain part of AVPR #115.
