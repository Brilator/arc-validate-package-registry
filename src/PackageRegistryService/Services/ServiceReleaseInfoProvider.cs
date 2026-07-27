using System.Reflection;
using System.Text.RegularExpressions;
using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

public sealed partial class ServiceReleaseInfoProvider : IServiceReleaseInfoProvider
{
    private const string ReleaseNotesFileName = "RELEASE_NOTES.md";
    private const string ReleaseNotesUrl = "/releases";
    public ServiceReleaseInfoProvider(
        IConfiguration configuration,
        IMarkdownRenderer markdownRenderer
    )
    {
        var assembly = typeof(ServiceReleaseInfoProvider).Assembly;
        var informationalVersion =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? assembly.GetName().Version?.ToString(3)
            ?? "0.0.0";

        var version = informationalVersion.Split('+', 2)[0];
        var revision =
            configuration["AVPR_BUILD_REVISION"]
            ?? RevisionFromInformationalVersion(informationalVersion)
            ?? "unknown";
        var channel = configuration["AVPR_BUILD_CHANNEL"] ?? "local";
        var created = ParseCreated(configuration["AVPR_BUILD_CREATED"]);

        var releaseNotesMarkdown = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, ReleaseNotesFileName)
        );
        var release = FindRelease(releaseNotesMarkdown, version);

        Current = new ServiceVersionDocument(
            new ServiceIdentity("avpr", version),
            new ApiIdentity(["v1"]),
            new BuildIdentity(revision, channel, created),
            new ReleaseIdentity(release.Name, release.Summary, ReleaseNotesUrl)
        );
        ReleaseNotesHtml = markdownRenderer.Render(releaseNotesMarkdown);
    }

    public ServiceVersionDocument Current { get; }

    public string ReleaseNotesHtml { get; }

    private static string? RevisionFromInformationalVersion(string informationalVersion)
    {
        var separator = informationalVersion.IndexOf('+');
        return separator >= 0 && separator < informationalVersion.Length - 1
            ? informationalVersion[(separator + 1)..]
            : null;
    }

    private static DateTimeOffset? ParseCreated(string? value) =>
        DateTimeOffset.TryParse(value, out var created) ? created : null;

    private static (string Name, string Summary) FindRelease(string markdown, string version)
    {
        var lines = markdown.ReplaceLineEndings("\n").Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            var match = ReleaseHeading().Match(lines[index]);
            if (!match.Success || match.Groups["version"].Value != version)
            {
                continue;
            }

            var summaryLines = lines
                .Skip(index + 1)
                .SkipWhile(string.IsNullOrWhiteSpace)
                .TakeWhile(line =>
                    !string.IsNullOrWhiteSpace(line)
                    && !line.StartsWith('#')
                    && !line.StartsWith('-')
                )
                .Select(line => line.Trim());
            var summary = string.Join(" ", summaryLines);

            return (
                match.Groups["name"].Value.Trim(),
                summary.Length > 0
                    ? summary
                    : $"See the release notes for version {version}."
            );
        }

        return ($"Version {version}", $"See the release notes for version {version}.");
    }

    [GeneratedRegex(
        @"^##\s+\[?(?<version>\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?)\]?\s+(?:—|-)\s+(?<name>.+?)\s*$"
    )]
    private static partial Regex ReleaseHeading();
}
