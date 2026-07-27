# Development setup

## Contents

- [Prerequisites](#prerequisites)
- [Documentation](#documentation)
- [Libraries](#libraries)
- [Registry service with Docker Compose](#registry-service-with-docker-compose)
- [Registry service with `dotnet watch`](#registry-service-with-dotnet-watch)
- [Changing the metadata or database model](#changing-the-metadata-or-database-model)

## Prerequisites

- .NET SDK pinned by `global.json` (currently .NET 10)
- Docker and Docker Compose
- `uv` when developing or checking Python validation packages

The main solution contains the registry service, index and client libraries,
CLI, and their tests. `PackageStagingArea.sln` contains the staging-area checks.

## Documentation

Long-form documentation lives under `docs/` as ordinary Markdown. The registry
service publishes those same files and renders them at `/docs`; no separate
documentation generator or generated copy is involved.

Keep links between documentation pages relative and include the `.md` suffix,
for example `[testing changes](testing.md)`. That single link form works both in
the deployed documentation site and while browsing the repository on GitHub.
Use an absolute production URL only for runtime-only destinations such as
Swagger or `/_version`.

## Libraries

`AVPRIndex` contains validation-package domain types, frontmatter parsing,
hashing, and index utilities. `AVPRClient` is the generated and consumer-facing
.NET client for the registry API.

Build or test them through the main solution:

```shell
dotnet build arc-validate-package-registry.sln --configuration Release
dotnet test arc-validate-package-registry.sln --configuration Release
```

For a NuGet release:

1. Bump the package version in the corresponding `.fsproj` or `.csproj`.
2. Update that project's `RELEASE_NOTES.md`.
3. Merge to `main`; CI publishes eligible packages to NuGet.

## Registry service with Docker Compose

From the repository root, start the same application, PostgreSQL, and Adminer
stack used by the Visual Studio Docker Compose project:

```shell
docker compose up --build
```

The development override maps the service's port to a dynamically selected host
port. Find it with:

```shell
docker compose port packageregistryservice 8080
```

If the command prints `0.0.0.0:54321`, browse to
`http://localhost:54321/swagger`. Adminer is available at
`http://localhost:8080`.

Common commands:

```shell
# Start in the background
docker compose up --build --detach

# Inspect containers and assigned ports
docker compose ps

# Follow service logs
docker compose logs --follow packageregistryservice

# Stop while retaining containers
docker compose stop

# Remove Compose containers
docker compose down
```

`ASPNETCORE_ENVIRONMENT` is `Development` in the Compose override, so the
service applies migrations and seeds the local database during startup.
Production startup does not apply migrations automatically.

## Registry service with `dotnet watch`

For a faster VS Code edit/rebuild loop, run only the dependencies in Docker and
run the service on the host. In PowerShell:

```powershell
docker compose up --detach package_db adminer

$env:ConnectionStrings__PostgressConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=employee"

dotnet watch --project src/PackageRegistryService/PackageRegistryService.csproj run --launch-profile http
```

The host service is then available at `http://localhost:5099`. Stop `dotnet
watch` with Ctrl+C and stop the dependency containers with `docker compose
stop`.

## Changing the metadata or database model

A validation-package metadata field commonly crosses several projects. Search
for every use and update the relevant layers:

- `src/AVPRIndex/Domain.fs` and frontmatter conversion;
- `src/PackageRegistryService/Models/ValidationPackage.cs`;
- Entity Framework ownership in `ValidationPackageDb.cs`;
- database seeding in `DataInitializer.cs`;
- generated client code and index/client mappings;
- website rendering when the field is user-facing;
- fixtures, hashes, contract tests, and documentation.

Generate migrations with the EF tooling rather than writing them from scratch,
then inspect the generated operations and model snapshot. Production migration
SQL is applied manually before deploying the matching image revision.

See [testing changes](testing.md) for the required verification path and
[operations and releases](../operations/releases.md) for publication details.
