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
- NuGet publication for versioned index/client releases;
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

## NuGet releases

For `AVPRIndex` or `AVPRClient`:

1. Update its project package version.
2. Update its `RELEASE_NOTES.md`.
3. Run the main solution build and tests.
4. Merge to `main`; the pipeline publishes the package when its release-note
   trigger and other gates pass.

## Publishing validation packages

Package publication is a separate process from deploying the registry service.
Inspect pending publication without pushing:

```shell
dotnet run --project src/AVPRCI/AVPRCI.fsproj -- publish --api-key <key> --dry-run
```

Only an authorized maintainer should remove `--dry-run` and perform production
publication. Never print or commit the API key. Published package versions are
immutable; corrections require a higher package version.

See [submitting packages](../packages/submission.md) for the contributor-facing
workflow.
