namespace PackageRegistryService.Models;

public sealed record DocumentationPage(
    string Title,
    string Html,
    IReadOnlyList<MarkdownHeading> Headings
);
