# ARC validation package registry

The ARC validation package registry (AVPR) is the source, staging area, and
service implementation for validation packages used by
[`arc-validate`](https://github.com/nfdi4plants/arc-validate) and DataHUB
continuous-quality-control pipelines.

The repository contains:

- `StagingArea/`: reviewed F# and Python validation-package submissions;
- `src/ValidationPackage.Model/`: portable validation-package domain types for
  .NET, JavaScript, and Python;
- `src/AVPRIndex/`: package metadata, frontmatter, hashing, and index utilities;
- `src/AVPRClient/`: the generated .NET registry client;
- `src/AVPRCI/`: publication tooling;
- `src/PackageRegistryService/`: the registry API and package-browser website;
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

The SDK is pinned by `global.json` (currently .NET 10). Build and test the main
solution with:

```shell
dotnet build arc-validate-package-registry.sln --configuration Release
dotnet test arc-validate-package-registry.sln --configuration Release
```

Run staging-area checks with:

```shell
dotnet build PackageStagingArea.sln --configuration Release
dotnet test PackageStagingArea.sln --configuration Release --no-build
```

Run the portable model contract suite on all three targets with:

```shell
dotnet tool restore
uv sync --locked
dotnet run --project tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --configuration Release
dotnet fable tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --outDir artifacts/model-tests/js --lang javascript --noCache
npm run test:model:js
dotnet fable tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --outDir artifacts/model-tests/py --lang python --noCache
uv run --locked python artifacts/model-tests/py/main.py
```

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
