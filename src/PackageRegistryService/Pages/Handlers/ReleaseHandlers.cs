using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Pages.Components;
using PackageRegistryService.Services;

namespace PackageRegistryService.Pages.Handlers;

public static class ReleaseHandlers
{
    public static ContentHttpResult Render(IServiceReleaseInfoProvider releaseInfo)
    {
        var content = Layout.Render(
            activeNavbarItem: "Releases",
            title: "AVPR service releases",
            content: Releases.Render(releaseInfo.Current, releaseInfo.ReleaseNotesHtml)
        );

        return TypedResults.Text(content: content, contentType: "text/html");
    }
}
