# CWL command inputs

## Contents

- [Why a CWL subset](#why-a-cwl-subset)
- [Example](#example)
- [Supported input fields](#supported-input-fields)
- [Supported types](#supported-types)
- [Supported binding fields](#supported-binding-fields)
- [Deliberately unsupported features](#deliberately-unsupported-features)
- [Representation boundaries](#representation-boundaries)

`Inputs` declares package-specific configurable inputs using a deliberately
scoped subset of
[CWL v1.2 `CommandLineTool.inputs`](https://www.commonwl.org/v1.2/CommandLineTool.html).
The AVPR wrapper remains PascalCase `Inputs`, consistently with the other
[package metadata](metadata.md). Its value uses CWL's array form and can be
placed under the lowercase `inputs` field of a complete CWL
`CommandLineTool`.

AVPR publishes a compatible CWL fragment; package frontmatter is not a complete
CWL document, and the registry is not a general CWL runner.

## Why a CWL subset

Package inputs need more than documentation: downstream tools must be able to
validate values and eventually construct deterministic command lines. Reusing
CWL provides established names and semantics for types, nullability, prefixes,
positional values, ordering, and value separation instead of creating an
AVPR-only schema.

The first release supports the configuration needed by current validation
scripts while keeping parsing, persistence, OpenAPI, generated clients, and
future argument construction unambiguous:

- Six scalar primitives map directly to portable command-line values.
- One nullable shorthand avoids multiple wire shapes for the same meaning.
- Three binding fields cover flags, options, concatenated values, and
  positional values.
- Array form maps directly to the registry's ordered collection and owned JSON
  model while retaining explicit input IDs.

Complex CWL types are deferred until the registry and downstream runner can
implement their staging, path, validation, and serialization semantics.

## Example

```yaml
Inputs:
  - id: echo
    type: string?
    label: Echo text
    doc: Print the supplied text
    inputBinding:
      prefix: --echo

  - id: verbose
    type: boolean?
    label: Verbose logging
    doc: Enable verbose logging
    inputBinding:
      prefix: --verbose

  - id: output
    type: string
    doc: Select the output file
    inputBinding:
      position: 2
      prefix: --output=
      separate: false
```

## Supported input fields

| CWL field | Supported value | Mandatory |
| --- | --- | --- |
| `id` | Non-empty string, unique within the package | yes |
| `type` | Supported scalar string, optionally followed by one `?` | yes |
| `label` | string | no |
| `doc` | string | no |
| `inputBinding` | Supported binding object | yes |

Nested names remain exactly lower-camel-case in frontmatter, public JSON,
OpenAPI, and generated-client serialization.

## Supported types

- `boolean`
- `int`
- `long`
- `float`
- `double`
- `string`

Appending exactly one `?` makes a value nullable, for example `boolean?` or
`string?`. This shorthand is the only accepted nullable representation. CWL
also permits union arrays such as `type: ["null", "string"]`, but accepting
both would expose two public shapes for the same meaning and complicate schema
generation, clients, and storage.

Do not add a separate `Required` property. Requiredness is represented by the
type: `string` requires a value, while `string?` permits omission or null.

## Supported binding fields

| CWL field | Supported value | Default |
| --- | --- | --- |
| `prefix` | One canonical string; omission makes the value positional | no prefix |
| `position` | integer | `0` |
| `separate` | boolean | `true` |

Binding behavior follows CWL:

- A boolean `true` emits its prefix; `false` emits nothing. A nullable boolean
  also emits nothing for null. Boolean flags never emit a trailing `true` or
  `false` value.
- A prefixed non-boolean with `separate: true` emits prefix and value as two
  argv elements.
- `separate: false` concatenates them, such as `--output=result.txt`.
- Omitting `prefix` emits a positional value.
- Missing `position` uses position `0`; equal positions are resolved
  deterministically by input ID.

CWL defines one binding prefix, not aliases. A package script may independently
accept `-v` as well as `--verbose`, but only one canonical prefix belongs in
the structured contract.

## Deliberately unsupported features

The first subset does not support:

- `File`, `Directory`, or `stdin`, which need path and staging semantics;
- arrays, records, and enums;
- union-array syntax or general unions;
- user-defined or IRI types;
- CWL maps keyed by input ID.

Additional fields on otherwise valid parameter or binding objects—such as
`default`, `secondaryFiles`, `format`, `valueFrom`, `itemSeparator`, and
`shellQuote`—are tolerated but ignored and discarded. This provides limited
forward tolerance without claiming support. Malformed known fields and
unsupported `type` values or shapes still fail with an actionable diagnostic.

## Representation boundaries

- Frontmatter and public API JSON expose `type` as one scalar such as
  `boolean?`.
- OpenAPI enumerates the twelve required/nullable scalar strings.
- The generated client maps that scalar to its client representation.
- PostgreSQL alone stores a normalized primitive/nullability object inside the
  owned `Inputs` JSON document.

The internal database object must never leak through the API. Existing packages
without `Inputs` are represented by an empty collection, including rows
backfilled during migration.

Declaring inputs does not install an argument parser in a package script.
Scripts remain responsible for interpreting argv until downstream execution
support lands in `arc-validate`.
