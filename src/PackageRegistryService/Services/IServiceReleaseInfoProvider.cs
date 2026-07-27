using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

public interface IServiceReleaseInfoProvider
{
    ServiceVersionDocument Current { get; }

    string ReleaseNotesHtml { get; }
}
