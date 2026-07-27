using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;
using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

public sealed partial class DocumentationProvider : IDocumentationProvider, IDisposable
{
    private const string DefaultTitle = "AVPR documentation";
    private readonly PhysicalFileProvider _files;
    private readonly IMarkdownRenderer _markdown;

    public DocumentationProvider(IMarkdownRenderer markdown)
    {
        _markdown = markdown;
        _files = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "docs"));
    }

    public DocumentationPage? GetPage(string document)
    {
        var path = NormalizeDocumentPath(document);
        if (path is null)
        {
            return null;
        }

        var file = _files.GetFileInfo(path);
        if (!file.Exists || file.IsDirectory)
        {
            return null;
        }

        using var reader = new StreamReader(file.CreateReadStream());
        var source = reader.ReadToEnd();
        var title = TitleHeading().Match(source) is { Success: true } match
            ? match.Groups["title"].Value.Trim()
            : DefaultTitle;

        var rendered = _markdown.RenderDocumentation(source);
        return new DocumentationPage(title, rendered.Html, rendered.Headings);
    }

    public void Dispose() => _files.Dispose();

    private static string? NormalizeDocumentPath(string document)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return null;
        }

        var segments = document.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (
            Path.IsPathRooted(document)
            || document.Contains('\\')
            || segments.Any(segment => segment is "." or "..")
        )
        {
            return null;
        }

        var path = string.Join('/', segments);
        return Path.GetExtension(path) switch
        {
            "" => $"{path}.md",
            ".md" => path,
            _ => null
        };
    }

    [GeneratedRegex(@"^#\s+(?<title>.+?)\s*$", RegexOptions.Multiline)]
    private static partial Regex TitleHeading();
}
