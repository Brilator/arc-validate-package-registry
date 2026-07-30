# CI/CD and releases

## Contents

- [Branch roles](#branch-roles)
- [Service versions and release notes](#service-versions-and-release-notes)
- [Manual service deployment](#manual-service-deployment)
- [NuGet releases](#nuget-releases)
- [Publishing validation packages](#publishing-validation-packages)

The GitHub Actions pipeline runs for pushes and pull requests targeting `main`
or `dev`. Its change detection selects the relevant jobs:

- main solution builds and tests for index, client, service, or test changes;
- staging-area syntax and metadata checks for package changes;
- pre-publication integrity checks and eligible package publication;
- trusted NuGet publication for versioned model/codecs/index/client releases;
- registry-service container publication.

Workflow actions, permissions, and release branches are defined under
`.github/workflows/` in the repository.

## Branch roles

- `dev` is the long-lived integration branch. Service changes publish the
  `ghcr.io/nfdi4plants/avpr:dev` image for the development instance.
- `main` is the production branch. Service changes publish
  `ghcr.io/nfdi4plants/avpr:main`; NuGet and production package releases are
  gated to this branch.

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

## NuGet releases

For `ValidationPackage.Model`, `ValidationPackage.Codecs`, `AVPRIndex`, or
`AVPRClient`:

1. Update its project package version. For `ValidationPackage.Model`, update
   the version in the packed-package smoke project's `PackageReference` too.
2. Update its `RELEASE_NOTES.md`.
3. Run `./build.sh TestSolution` (or `.\build.cmd TestSolution` on Windows).
4. For `ValidationPackage.Model` or `ValidationPackage.Codecs`, run its
   `TestPortableModel` or `TestPortableCodecs` build target described in
   [testing changes](../development/testing.md).
5. Merge to `main`; the pipeline publishes the package when its release-note
   trigger and other gates pass. The model release additionally requires its
   cross-target and packed-consumer checks.

NuGet publishing is keyless. The reusable release job enters the `release`
GitHub environment, requests an OIDC token, and exchanges it through
`NuGet/login` for a one-hour, single-use API key immediately before pushing.
No long-lived NuGet API key is stored in GitHub.

Repository administrators must configure both sides of the trust relationship:

1. Create a GitHub Actions environment named `release`. Optional required
   reviewers on this environment turn package publication into an approval
   gate without changing the workflow.
2. Add a nuget.org trusted-publishing policy with repository owner
   `nfdi4plants`, repository `arc-validate-package-registry`, workflow file
   `release-package.yml`, and environment `release`. Enter only the workflow
   filename, not `.github/workflows/release-package.yml`.
3. Store the nuget.org profile name associated with that policy as the
   repository secret `NUGET_USER`. This is an identifier, not an API key.
4. Remove the obsolete `NUGET_KEY` secret after the trusted publisher has been
   configured and a release has succeeded.

The package jobs are selected by `pipeline.yml`, but publication executes in
the reusable `release-package.yml` workflow. NuGet validates GitHub's
`job_workflow_ref` OIDC claim, which identifies that called workflow. Re-running
a partially successful release is safe because pushes use `--skip-duplicate`.

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
