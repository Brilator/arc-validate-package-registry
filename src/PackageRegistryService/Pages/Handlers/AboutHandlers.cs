using Microsoft.AspNetCore.Http.HttpResults;

namespace PackageRegistryService.Pages.Handlers;

public static class AboutHandlers
{
    public static RedirectHttpResult Render() =>
        TypedResults.Redirect("/docs/index.md#about-avpr", permanent: true);
}
