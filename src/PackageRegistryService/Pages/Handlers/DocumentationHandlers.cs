using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Pages.Components;
using PackageRegistryService.Services;

namespace PackageRegistryService.Pages.Handlers;

public static class DocumentationHandlers
{
    public static RedirectHttpResult RenderIndex() =>
        TypedResults.Redirect("/docs/index.md", permanent: false);

    public static Results<ContentHttpResult, NotFound> Render(
        string document,
        IDocumentationProvider documentation
    )
    {
        var page = documentation.GetPage(document);
        if (page is null)
        {
            return TypedResults.NotFound();
        }

        var content = Layout.Render(
            activeNavbarItem: "Documentation",
            title: Documentation.RenderTitle(page),
            content: Documentation.Render(page),
            additionalHeadContent: @"<link rel=""stylesheet"" href=""/css/documentation.css"" />"
        );

        return TypedResults.Text(content: content, contentType: "text/html");
    }
}
