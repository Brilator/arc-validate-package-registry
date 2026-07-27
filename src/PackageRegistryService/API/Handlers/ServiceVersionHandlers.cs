using Microsoft.AspNetCore.Http.HttpResults;
using PackageRegistryService.Models;
using PackageRegistryService.Services;

namespace PackageRegistryService.API.Handlers;

public static class ServiceVersionHandlers
{
    public static Ok<ServiceVersionDocument> Get(
        HttpResponse response,
        IServiceReleaseInfoProvider releaseInfo
    )
    {
        response.Headers.CacheControl = "no-store";
        return TypedResults.Ok(releaseInfo.Current);
    }
}
