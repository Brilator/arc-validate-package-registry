# Testing changes

## Contents

- [Common commands](#common-commands)
- [Portable model](#portable-model)
- [In-process registry test host](#in-process-registry-test-host)
- [Metadata-model changes](#metadata-model-changes)

Use focused tests while iterating, then run the affected solution before
handoff.

## Common commands

```shell
# Main libraries, service, CLI, and tests
dotnet build arc-validate-package-registry.sln --configuration Release
dotnet test arc-validate-package-registry.sln --configuration Release

# Staging-area checks
dotnet build PackageStagingArea.sln --configuration Release
dotnet test PackageStagingArea.sln --configuration Release --no-build

# Focused suites
dotnet test tests/IndexTests/IndexTests.fsproj --configuration Release
dotnet test tests/ClientTests/ClientTests.fsproj --configuration Release
dotnet test tests/APITests/APITests.csproj --configuration Release
dotnet test StagingAreaTests/StagingAreaTests.fsproj --configuration Release
```

## Portable model

`ValidationPackage.Model.Tests` runs the same behavioral contract on .NET,
JavaScript, and Python. It is a Pyxpecto executable, so invoke the .NET target
with `dotnet run` rather than `dotnet test`.

```shell
dotnet tool restore
uv sync --locked

dotnet run --project tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --configuration Release

dotnet fable tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --outDir artifacts/model-tests/js --lang javascript --noCache
npm run test:model:js

dotnet fable tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --outDir artifacts/model-tests/py --lang python --noCache
uv run --locked python artifacts/model-tests/py/main.py
```

Generated output belongs under ignored `artifacts/`. After changing a public
portable type, inspect that output for attached class members, clean backing
field names, and target-specific behavior. CI also packs the model, verifies
that its ordered F# sources are present under `fable/`, and restores a smoke
consumer from the local package rather than a project reference.

## In-process registry test host

`tests/PackageRegistryTestHost` is shared infrastructure rather than a test
suite. `PackageRegistryWebApplicationFactory` starts the real registry entry
point through `WebApplicationFactory<Program>`, so requests exercise production
endpoint mappings, handlers, filters, and JSON configuration.

The factory uses the `Testing` environment. It disables development migrations,
staging-data initialization, the external PostgreSQL health check, and HTTPS
redirection, then replaces Npgsql with a uniquely named EF in-memory database.
Create and dispose one factory per test or fixture.

Seed through `SeedPackageAsync` or `SeedPackagesAsync`. These helpers add the
matching integrity-hash and zero-download records required by the real handlers.

Use the host at two boundaries:

- API tests call `factory.CreateClient()` and assert status codes and exact raw
  JSON property/value shapes.
- Generated-client tests pass that same `HttpClient` to `AVPRClient.Client`,
  replace its production `BaseUrl`, invoke the generated operation, and assert
  the typed result.

The in-memory provider does not verify PostgreSQL migrations, `jsonb` layout,
backfills, Npgsql conversion, or relational behavior. Retain focused EF model
assertions and manually verify persistence changes against PostgreSQL.

## Metadata-model changes

A metadata field crosses multiple representations. A green build alone does
not prove parsing or wire behavior.

1. **Domain behavior:** update mandatory/default and all-fields reference values
   in `tests/IndexTests/ReferenceObjects.fs`. Cover factories, defaults,
   equality, and hashing in `DomainTests.fs`.
2. **Frontmatter:** update full source, extracted YAML, expected metadata, and
   fixtures for F#/Python comment and binding forms. Preserve a no-field case
   for optional-field compatibility and add focused malformed cases.
3. **Hashes and indexes:** recompute hashes whenever fixture bytes change and
   update expected package indexes. Verify both metadata and hash values.
4. **Generated client:** regenerate from the local service OpenAPI document,
   maintain equivalent client/index reference objects, and test both mapping
   directions, nested collections, and null/empty behavior.
5. **Staging compatibility:** run `PackageStagingArea.sln` when submitted script
   syntax or checks are affected. Add a real staged package only when layout
   itself must be exercised, always under a new semantic version.
6. **Service boundaries:** test exact raw JSON and generated-client
   deserialization through the in-process host. For persistence changes, inspect
   migrations and backfills against PostgreSQL and check website rendering with
   both present and absent values.

Do not weaken expected values to make tests pass. Recompute them from the actual
fixtures and preserve older fixtures that demonstrate backwards compatibility.
