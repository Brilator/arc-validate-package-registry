using System.Net;
using System.Text;
using PackageRegistryService.Models;

namespace PackageRegistryService.Pages.Components;

public static class Documentation
{
    public static string Render(DocumentationPage page) =>
        $@"<p><a href=""/docs"">Documentation home</a></p>
<div class=""documentation-layout"">
  <aside class=""documentation-sidebar"">
    <nav aria-label=""On this page"">
      <strong>On this page</strong>
      {RenderTableOfContents(page.Headings)}
    </nav>
  </aside>
  <article class=""documentation-content"">
  {page.Html}
  </article>
</div>";

    public static string RenderTitle(DocumentationPage page) =>
        WebUtility.HtmlEncode(page.Title);

    private static string RenderTableOfContents(
        IReadOnlyList<MarkdownHeading> headings
    )
    {
        var content = new StringBuilder("<ul>");

        foreach (var heading in headings)
        {
            var id = WebUtility.HtmlEncode(heading.Id);
            var text = WebUtility.HtmlEncode(heading.Text);
            content.Append($@"<li class=""toc-level-{heading.Level}""><a href=""#{id}"">{text}</a></li>");
        }

        return content.Append("</ul>").ToString();
    }
}
