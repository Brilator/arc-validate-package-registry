using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using PackageRegistryService.Services;
using PackageRegistryTestHost;

namespace APITests;

public partial class DocumentationTests
{
    [Fact]
    public async Task DocumentationHomeUsesCanonicalUrlAndGitHubCompatibleLinks()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        using var client = factory.CreateClient(
            new() { AllowAutoRedirect = false }
        );

        using var redirect = await client.GetAsync("/docs");

        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        Assert.Equal("/docs/index.md", redirect.Headers.Location?.OriginalString);

        using var response = await client.GetAsync("/docs/index.md");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("ARC validation package registry documentation", html);
        Assert.Contains("href=\"packages/submission.md\"", html);
        Assert.Contains("id=\"about-avpr\"", html);
        Assert.Contains("Frequently asked questions", html);
        Assert.Contains("href=\"/css/documentation.css\"", html);
        Assert.Contains("class=\"documentation-sidebar\"", html);
        Assert.Contains("class=\"toc-level-3\"", html);
        Assert.Contains("href=\"#about-avpr\"", html);
        Assert.DoesNotContain("id=\"contents\"", html);
        Assert.Contains("aria-current=\"page\" href=\"/docs\"", html);

        using var stylesheet = await client.GetAsync("/css/documentation.css");
        stylesheet.EnsureSuccessStatusCode();
        Assert.Equal("text/css", stylesheet.Content.Headers.ContentType?.MediaType);
        var css = await stylesheet.Content.ReadAsStringAsync();
        Assert.Contains(
            "@media (max-width: 768px)",
            css
        );
        Assert.Contains(
            ".documentation-sidebar nav a",
            css
        );
        Assert.Contains("margin: calc(var(--pico-spacing) / 2) 0 0", css);
        Assert.Contains("overflow-x: hidden", css);
        Assert.Contains("overflow-wrap: anywhere", css);

        using var home = await client.GetAsync("/");
        home.EnsureSuccessStatusCode();
        Assert.DoesNotContain(
            "href=\"/css/documentation.css\"",
            await home.Content.ReadAsStringAsync()
        );
    }

    [Fact]
    public async Task LegacyAboutPageRedirectsToDocumentation()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        using var client = factory.CreateClient(
            new() { AllowAutoRedirect = false }
        );

        using var response = await client.GetAsync("/about");

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal(
            "/docs/index.md#about-avpr",
            response.Headers.Location?.OriginalString
        );
    }

    [Theory]
    [InlineData("/docs/packages/cwl-inputs.md")]
    [InlineData("/docs/packages/cwl-inputs")]
    public async Task DocumentationPageRendersMarkdown(string path)
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<h1", html);
        Assert.Contains("CWL command inputs", html);
        Assert.Contains("Why a CWL subset", html);
        Assert.Contains("<table>", html);
    }

    [Fact]
    public async Task MissingOrNonMarkdownDocumentReturnsNotFound()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        using var client = factory.CreateClient();

        using var missing = await client.GetAsync("/docs/not-found.md");
        using var nonMarkdown = await client.GetAsync("/docs/index.txt");

        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, nonMarkdown.StatusCode);
    }

    [Fact]
    public void DocumentationProviderRejectsPathsOutsideItsRoot()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        var provider = factory.Services.GetRequiredService<IDocumentationProvider>();

        Assert.Null(provider.GetPage("../README.md"));
        Assert.Null(provider.GetPage("packages\\metadata.md"));
        Assert.Null(provider.GetPage("C:/README.md"));
    }

    [Fact]
    public void RelativeMarkdownLinksResolveWithinTheDocumentationTree()
    {
        var docsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "docs"));
        var files = Directory.GetFiles(docsRoot, "*.md", SearchOption.AllDirectories);

        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            foreach (Match match in MarkdownLink().Matches(source))
            {
                var target = match.Groups["target"].Value;
                if (
                    target.StartsWith('#')
                    || Uri.TryCreate(target, UriKind.Absolute, out _)
                )
                {
                    continue;
                }

                var path = target.Split(['#', '?'], 2)[0]
                    .Replace('/', Path.DirectorySeparatorChar);
                var resolved = Path.GetFullPath(
                    Path.Combine(Path.GetDirectoryName(file)!, path)
                );

                Assert.StartsWith(
                    docsRoot + Path.DirectorySeparatorChar,
                    resolved,
                    StringComparison.OrdinalIgnoreCase
                );
                Assert.True(File.Exists(resolved), $"Broken link '{target}' in '{file}'.");
            }
        }
    }

    [Fact]
    public void EveryDocumentationPageHasAContentsSectionNearTheStart()
    {
        var docsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "docs"));
        var files = Directory.GetFiles(docsRoot, "*.md", SearchOption.AllDirectories);

        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            var firstLines = File.ReadLines(file).Take(20);
            Assert.Contains(
                "## Contents",
                firstLines,
                StringComparer.Ordinal
            );
        }
    }

    [Fact]
    public void TableOfContentsLinksMatchMarkdigHeadingIdentifiers()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        var provider = factory.Services.GetRequiredService<IDocumentationProvider>();
        var docsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "docs"));
        var files = Directory.GetFiles(docsRoot, "*.md", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var document = Path.GetRelativePath(docsRoot, file).Replace('\\', '/');
            var page = Assert.IsType<PackageRegistryService.Models.DocumentationPage>(
                provider.GetPage(document)
            );
            var source = File.ReadAllText(file);
            var contentsStart = source.IndexOf("## Contents", StringComparison.Ordinal);
            var contentsEnd = source.IndexOf(
                "\n## ",
                contentsStart + "## Contents".Length,
                StringComparison.Ordinal
            );
            var contents = source[contentsStart..contentsEnd];
            var anchors = MarkdownLink()
                .Matches(contents)
                .Select(match => match.Groups["target"].Value)
                .Where(target => target.StartsWith('#'))
                .ToArray();

            Assert.Equal(
                anchors,
                page.Headings.Select(heading => $"#{heading.Id}")
            );

            foreach (var anchor in anchors)
            {
                Assert.Contains($"id=\"{anchor[1..]}\"", page.Html);
            }
        }
    }

    [GeneratedRegex(@"\[[^\]]+\]\((?<target>[^)\s]+)(?:\s+[^)]*)?\)")]
    private static partial Regex MarkdownLink();
}
