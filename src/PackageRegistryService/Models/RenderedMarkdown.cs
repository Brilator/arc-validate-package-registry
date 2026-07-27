namespace PackageRegistryService.Models;

public sealed record MarkdownHeading(string Id, string Text, int Level);

public sealed record RenderedMarkdown(
    string Html,
    IReadOnlyList<MarkdownHeading> Headings
);
