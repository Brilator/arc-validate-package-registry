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

GitHub Actions automation runs for pushes and pull requests targeting `release`
or `dev`, split by responsibility:

- `ci.yml` runs path-focused main solution tests;
- `portable-ci.yml` runs Model and Codecs contract suites on .NET, JavaScript,
  and Python, including packed consumers;
- `staging-ci.yml` runs staging-area syntax and metadata checks on Windows;
- `service-image.yml` gates registry-service container publication on the main
  solution;
- package-specific manual workflows rerun their required gates before entering
  the protected publishing environment.

Routine `dev`, manual, and package-release verification runs the main solution
on Ubuntu. Pushes to `release` and pull requests targeting `release` add Windows and
macOS, providing a cross-platform gate without multiplying every development
build.

Workflow actions, permissions, and release branches are defined under
`.github/workflows/` in the repository.

## Branch roles

- `dev` is the default, long-lived integration branch. Service changes publish the
  `ghcr.io/nfdi4plants/avpr:dev` image for the development instance.
- `release` is the production branch. Service changes publish
  `ghcr.io/nfdi4plants/avpr:release`.

No branch push publishes library packages. Package releases are explicit
manual workflow dispatches from a reviewed repository revision and enter the
protected `release` environment before publication.

Each service build also publishes an immutable `sha-<short-sha>` tag. Prefer the
SHA tag for manual deployment; `release` and `dev` are convenient moving aliases.

## Service versions and release notes

The service version is maintained in
`src/PackageRegistryService/PackageRegistryService.csproj`. Human-readable
service changes are maintained in
`src/PackageRegistryService/RELEASE_NOTES.md`. The newest heading must use:

```text
## <version> - YYYY-MM-DD - <friendly release name>
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
built from `release`, `local`, or any other channel never use this deployed-dev
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

Release workflows rerun their required tests before publishing. No
`RELEASE_NOTES.md` change or branch push publishes a library package
automatically.

Prerelease notes use one rolling top entry: bump that entry in place and keep
accumulating the unreleased preview changes beneath it. Once a non-preview
version is published, its release-notes entry is immutable.

To release every new package version in this repository, manually dispatch
`release-all.yml` against the intended ref. It validates the version gate,
checks the exact committed versions on each package's target registries, and
dispatches only package workflows whose version is missing from at least one
registry. It does not publish directly, so the package-specific trusted
publisher configuration below remains unchanged. The run summary reports the
registry state and action for every package. It uses the repository's temporary
`GITHUB_TOKEN` to dispatch workflows in this repository; no additional token or
secret is required.

### Portable package releases

`release-model.yml` and `release-codecs.yml` each run `TestSolution` and the
package's portable target. The portable target tests the package on all three
runtimes and leaves the exact verified NuGet package, npm tarball, and Python
wheel that the workflow preserves and publishes through three independent
registry jobs. Release Model before Codecs when both versions change because
Codecs depends on Model in all three ecosystems.

The workflows are manually dispatched and have no path filter, so any reviewed
fix commit can be released even when it does not touch package paths. If one
registry fails, use **Re-run failed jobs** on the existing workflow run. The
successful registry jobs are not rerun, and the failed job downloads the exact
artifact produced by the original verification job. Full reruns are also safe: NuGet
uses `--skip-duplicate`, npm checks whether the exact package version already
exists, and PyPI uses `skip-existing`.

The JavaScript package names are npm-scoped, but this does not change emitted
type or namespace names. Python distribution names use hyphens while Python
imports use `validation_package_model` and `validation_package_codecs`.

### Client package releases

`release-client.yml` and `release-client-interop.yml` rerun `TestSolution`,
then call the reusable `release-package.yml` NuGet publisher with `PackClient`
or `PackClientInterop`. Interop remains independently
versioned so consumers can opt into portable model conversion.

### Trusted-publisher configuration

Create a protected GitHub environment named `release` in both repositories.
Add required reviewers and allow deployments from both `dev` and `release` so
the manually dispatched package workflows can publish reviewed preview versions
from `dev` as well as production versions from `release`. Create one repository
Actions variable, `NUGET_USER`, whose value is the nuget.org profile that owns
the trusted-publishing policies (currently `Mutagene`), not an email address,
GitHub username, organization name, or API key.

On nuget.org, select the personal account `Mutagene` as the policy owner and
keep the NuGet packages owned by that account. NuGet policies are owner-wide:
they cannot be restricted to an individual package. Consequently, each policy
below can technically publish any package owned by `Mutagene`; the package
column documents the intended use of the authorized workflow, not a NuGet
enforcement boundary. Create these policies; workflow values are filenames
only:

| Intended packages | Repository owner | Repository | Workflow file | Environment |
| --- | --- | --- | --- | --- |
| `ValidationPackage.Model` | `nfdi4plants` | `arc-validate-package-registry` | `release-model.yml` | `release` |
| `ValidationPackage.Codecs` | `nfdi4plants` | `arc-validate-package-registry` | `release-codecs.yml` | `release` |
| `AVPRClient`, `AVPRClient.Interop` | `nfdi4plants` | `arc-validate-package-registry` | `release-package.yml` | `release` |
| `ARCExpect` | `nfdi4plants` | `arc-validate` | `release-arcexpect.yml` | `release` |

The Client and Interop policy names the called reusable workflow because that
is the workflow NuGet observes in the `job_workflow_ref` claim. The reusable
workflow reads the `NUGET_USER` repository variable directly, so no secret needs
to be forwarded through `workflow_call`. The four policy rows distinguish
trusted workflow identities, but they do not create four package-level scopes.

On npmjs.com, open each package's **Settings → Trusted Publisher**, select
**GitHub Actions**, and enter:

| npm package | Organization or user | Repository | Workflow filename | Environment | Allowed actions |
| --- | --- | --- | --- | --- | --- |
| `@nfdi4plants/validationpackage-model` | `nfdi4plants` | `arc-validate-package-registry` | `release-model.yml` | `release` | `npm publish` |
| `@nfdi4plants/validationpackage-codecs` | `nfdi4plants` | `arc-validate-package-registry` | `release-codecs.yml` | `release` | `npm publish` |
| `@nfdi4plants/arcexpect` | `nfdi4plants` | `arc-validate` | `release-arcexpect.yml` | `release` | `npm publish` |

Each npm package can have only one trusted publisher. The values are
case-sensitive and the workflow filename includes `.yml`. No `NPM_TOKEN`
secret is used. Keep the packages in the `nfdi4plants` npm scope and grant
maintainer access through the npm organization rather than a personal scope.
The workflows explicitly publish prerelease versions with the `preview`
dist-tag and stable versions with `latest`; npm rejects prerelease publication
without a non-`latest` tag.

On pypi.org, open each project under **Manage → Publishing**, add a GitHub
Actions trusted publisher, and enter:

| PyPI project | Owner | Repository | Workflow name | Environment |
| --- | --- | --- | --- | --- |
| `validationpackage-model` | `nfdi4plants` | `arc-validate-package-registry` | `release-model.yml` | `release` |
| `validationpackage-codecs` | `nfdi4plants` | `arc-validate-package-registry` | `release-codecs.yml` | `release` |
| `arcexpect` | `nfdi4plants` | `arc-validate` | `release-arcexpect.yml` | `release` |

Use the pending-publisher form with the same values if a PyPI project does not
exist yet. PyPI does not support registering a reusable workflow as the
publisher, so these must remain top-level workflows with their own publish
jobs.

The resulting package-to-workflow map is:

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
| `ARCExpect` | NuGet | `release-arcexpect.yml` |
| `@nfdi4plants/arcexpect` | npm | `release-arcexpect.yml` |
| `arcexpect` | PyPI | `release-arcexpect.yml` |

For PyPI, the workflow is top-level and contains the publish job directly; do
not move it into a reusable workflow. Model and Codecs publish directly from
their top-level package workflows, so their NuGet and npm policies name those
workflows.

The publish jobs request `id-token: write`. `NuGet/login` exchanges OIDC for a
temporary API key; npm and PyPI also exchange OIDC without repository tokens.
Store only the nuget.org profile name in the `NUGET_USER` repository Actions
variable, and do not restore a long-lived `NUGET_KEY`, npm token, or PyPI token.

See the official registry instructions for
[NuGet trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing),
[npm trusted publishers](https://docs.npmjs.com/trusted-publishers/), and
[PyPI trusted publishers](https://docs.pypi.org/trusted-publishers/adding-a-publisher/).

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
