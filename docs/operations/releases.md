# CI/CD and releases

## Contents

- [Branch roles](#branch-roles)
- [Service versions and release notes](#service-versions-and-release-notes)
- [Manual service deployment](#manual-service-deployment)
- [Package releases](#package-releases)
  - [Portable package releases](#portable-package-releases)
  - [Client package releases](#client-package-releases)
  - [Trusted-publisher configuration](#trusted-publisher-configuration)
  - [AVPRIndex retirement](#avprindex-retirement)
- [Publishing validation packages](#publishing-validation-packages)

GitHub Actions automation runs for pushes and pull requests targeting `main`
or `dev`, split by responsibility:

- `ci.yml` runs path-focused main solution tests and downstream compatibility;
- `portable-ci.yml` runs Model and Codecs contract suites on .NET, JavaScript,
  and Python, including packed consumers;
- `staging-ci.yml` runs staging-area syntax and metadata checks on Windows;
- `service-image.yml` gates registry-service container publication on the main
  solution and downstream compatibility;
- package-specific manual workflows rerun their required gates before entering
  the protected publishing environment.

Workflow actions, permissions, and release branches are defined under
`.github/workflows/` in the repository.

## Branch roles

- `dev` is the long-lived integration branch. Service changes publish the
  `ghcr.io/nfdi4plants/avpr:dev` image for the development instance.
- `main` is the production branch. Service changes publish
  `ghcr.io/nfdi4plants/avpr:main`.

No branch push publishes library packages. Package releases are explicit
manual workflow dispatches from a reviewed repository revision and enter the
protected `release` environment before publication.

Each service build also publishes an immutable `sha-<short-sha>` tag. Prefer the
SHA tag for manual deployment; `main` and `dev` are convenient moving aliases.

## Service versions and release notes

The service version is maintained in
`src/PackageRegistryService/PackageRegistryService.csproj`. Human-readable
service changes are maintained in
`src/PackageRegistryService/RELEASE_NOTES.md`. The newest heading must use:

```text
## [<version>] - <friendly release name>
```

and match the project version.

The image build records its full Git commit as the .NET `SourceRevisionId` and
as OCI metadata. The running service exposes its service/API version, full
revision, source channel, creation time, friendly release name, and notes link
at `/_version`. The release page is available at `/releases`; `/_health`
remains the database-aware monitoring endpoint.

## Manual service deployment

Production and development deployment is currently manual:

1. Choose the immutable image matching the intended commit.
2. Generate and inspect migration SQL from that same repository revision.
3. Back up the database when appropriate.
4. Apply the migration SQL.
5. Pull and start the SHA-pinned image.
6. Verify `/_version`, `/_health`, Swagger, and relevant website behavior.

```shell
docker pull ghcr.io/nfdi4plants/avpr:sha-<short-sha>
```

Production startup deliberately does not apply migrations.

A service image built from the `dev` branch is the one exception for a new
development instance. When its configured database contains no tables, startup
applies the bundled migrations and seeds packages from `StagingArea`. If any
table already exists, startup does not migrate or seed the database. Images
built from `main`, `local`, or any other channel never use this deployed-dev
initialization path.

## Package releases

For every library package:

1. Update the project package version and `RELEASE_NOTES.md`. Update exact
   packed-consumer pins when required.
2. Run the focused target and `TestSolution` as described in
   [testing changes](../development/testing.md).
3. Push the reviewed revision.
4. Manually dispatch that package's release workflow against the intended ref.
5. Approve the protected `release` environment when configured to require it.

Release workflows rerun their required tests and downstream compatibility
before publishing. No `RELEASE_NOTES.md` change or branch push publishes a
library package automatically.

### Portable package releases

`release-model.yml` and `release-codecs.yml` each run the core, portable, and
downstream gates, then call `PackModel` or `PackCodecs` exactly once. That one
build produces the NuGet package, npm tarball, and Python wheel published by the
same job. Release Model before Codecs when both versions change because Codecs
depends on Model in all three ecosystems.

Retries are safe: NuGet uses `--skip-duplicate`, npm checks whether the exact
package version already exists before publishing, and PyPI uses
`skip-existing`.

The JavaScript package names are npm-scoped, but this does not change emitted
type or namespace names. Python distribution names use hyphens while Python
imports use `validation_package_model` and `validation_package_codecs`.

### Client package releases

`release-client.yml` and `release-client-interop.yml` rerun `TestSolution` and
downstream compatibility, then call the reusable `release-package.yml` NuGet
publisher with `PackClient` or `PackClientInterop`. Interop remains independently
versioned so consumers can opt into portable model conversion.

### Trusted-publisher configuration

All policies use repository owner `nfdi4plants`, repository
`arc-validate-package-registry`, and environment `release`.

| Package | Registry | Trusted workflow |
| --- | --- | --- |
| `ValidationPackage.Model` | NuGet | `release-model.yml` |
| `@nfdi4plants/validationpackage-model` | npm | `release-model.yml` |
| `validationpackage-model` | PyPI | `release-model.yml` |
| `ValidationPackage.Codecs` | NuGet | `release-codecs.yml` |
| `@nfdi4plants/validationpackage-codecs` | npm | `release-codecs.yml` |
| `validationpackage-codecs` | PyPI | `release-codecs.yml` |
| `AVPRClient` | NuGet | `release-package.yml` |
| `AVPRClient.Interop` | NuGet | `release-package.yml` |

For npm, allow the `npm publish` action. For PyPI, the workflow is top-level
and contains the publish job directly; do not move it into a reusable workflow.
For Client and Interop, NuGet validates the called `release-package.yml` through
the `job_workflow_ref` OIDC claim. Model and Codecs publish directly from their
top-level package workflows, so their NuGet policies name those workflows.

The publish jobs request `id-token: write`. `NuGet/login` exchanges OIDC for a
temporary API key; npm and PyPI also exchange OIDC without repository tokens.
Store only the nuget.org profile name as `NUGET_USER`, and do not restore a
long-lived `NUGET_KEY`, npm token, or PyPI token.

### AVPRIndex retirement

`AVPRIndex` is retired and is no longer built or published from this
repository. Existing versions on NuGet remain available for compatibility but
receive no further updates. Consumers should use `ValidationPackage.Model` for
domain types, `ValidationPackage.Codecs` for YAML/frontmatter and domain JSON,
and `AVPR.Staging` only for registry-side staged-package infrastructure.
Generated-client consumers that need portable model conversion should use
`AVPRClient.Interop`.

Published validation-package scripts that reference an existing `AVPRIndex`
version remain immutable and continue to resolve that historical package.

## Publishing validation packages

Package publication is a separate process from deploying the registry service.
Inspect pending publication without pushing:

```shell
dotnet run --project src/AVPRCI/AVPRCI.fsproj -- publish \
  --base-url https://avpr.nfdi4plants.org \
  --api-key <key> \
  --dry-run
```

`--base-url` is required so that the target registry is always explicit. To
populate an initialized but empty development registry from `StagingArea`, use
the development instance URL and its API key:

```shell
dotnet run --project src/AVPRCI/AVPRCI.fsproj -- publish \
  --base-url <development-registry-url> \
  --api-key <development-api-key>
```

AVPRCI verifies packages already present at the selected endpoint and publishes
only missing packages whose frontmatter has `Publish: true`. It does not create
or migrate the database schema. An empty database used by a `dev`-channel image
is initialized during service startup; otherwise apply the service migrations
before using AVPRCI against a new instance.

Only an authorized maintainer should remove `--dry-run` and perform production
publication. Never print or commit the API key. Published package versions are
immutable; corrections require a higher package version.

See [submitting packages](../packages/submission.md) for the contributor-facing
workflow.
