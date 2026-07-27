using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

public interface IMarkdownRenderer
{
    string Render(string markdown);

    RenderedMarkdown RenderDocumentation(string markdown);
}
