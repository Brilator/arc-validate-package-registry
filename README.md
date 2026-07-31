# ARC validation package registry

The ARC validation package registry (AVPR) is the source, staging area, and
service implementation for validation packages used by
[`arc-validate`](https://github.com/nfdi4plants/arc-validate) and DataHUB
continuous-quality-control pipelines.

The repository contains:

- `StagingArea/`: reviewed F# and Python validation-package submissions;
- `src/ValidationPackage.Model/`: portable validation-package domain types for
  .NET, JavaScript, and Python;
- `src/ValidationPackage.Codecs/`: portable YAML frontmatter and JSON codecs for
  the shared model on .NET, JavaScript, and Python;
- `src/AVPR.Staging/`: internal repository discovery, normalized content, and
  content hashing for staged validation packages;
- `src/AVPRClient/`: the generated .NET registry client;
- `src/AVPRClient.Interop/`: optional mappings between generated client DTOs and the portable model;
- `src/AVPRCI/`: publication tooling;
- `src/PackageRegistryService/`: the registry API and package-browser website;
- `build/`: the cross-platform build project used by local development and CI;
- `tests/` and `StagingAreaTests/`: library, API, contract, and package checks.

The production package browser and API are available at
[avpr.nfdi4plants.org](https://avpr.nfdi4plants.org).

## Documentation

The complete documentation is maintained as ordinary Markdown so it remains
readable both on GitHub and through the service's `/docs` pages:

- [Documentation home](docs/index.md)
- [Submit and version a validation package](docs/packages/submission.md)
- [Validation package metadata](docs/packages/metadata.md)
- [CWL command inputs](docs/packages/cwl-inputs.md)
- [Development setup](docs/development/overview.md)
- [Testing changes](docs/development/testing.md)
- [CI/CD and releases](docs/operations/releases.md)

Endpoint-level API documentation is provided by the deployed
[Swagger UI](https://avpr.nfdi4plants.org/swagger).

## Quick start

The SDK is pinned by `global.json` (currently .NET 10). The build project has
thin wrappers for Windows (`build.cmd`) and Unix (`build.sh`). Build and test
the main solution with:

```shell
./build.sh TestSolution
```

Run staging-area checks with:

```shell
./build.sh TestStagingArea
```

Run the portable model contract suite on all three targets with:

```shell
./build.sh TestPortableModel
```

Run the portable codec suite on all three targets with:

```shell
./build.sh TestPortableCodecs
```

Pack all NuGet, npm, and Python candidate artifacts together with:

```shell
./build.sh PackCandidatePackages
```

Before publishing a client, model, or codec change, AVPR CI checks out the
explicitly pinned `arc-validate` commit and runs its `TestAVPRCandidate` target
against these local artifacts. To reproduce that cross-repository gate, set
`AVPR_CANDIDATE_PACKAGE_DIR` to this repository's `artifacts/packages`
directory, then run `./build.sh TestAVPRCandidate` from the pinned
`arc-validate` checkout. Update the CI pin deliberately when the downstream
compatibility implementation changes.

On Windows, replace `./build.sh` with `.\build.cmd`.

With Docker Desktop running, start the registry service, PostgreSQL, and Adminer
development stack with:

```shell
docker compose up --build
```

Discover the dynamically assigned service port with:

```shell
docker compose port packageregistryservice 8080
```

See [development setup](docs/development/overview.md) for the VS Code
`dotnet watch` workflow and [testing changes](docs/development/testing.md) for
focused commands.

## Contributing packages

Each package is a self-contained `.fsx` or `.py` script under
`StagingArea/<package-name>/`. Its directory, filename, and frontmatter identity
must agree. Published versions are immutable, so updates use a new semantic
version.

Start with [submitting a validation package](docs/packages/submission.md) and
the [metadata reference](docs/packages/metadata.md).

## License

This repository is licensed under the terms in [LICENSE](LICENSE).
