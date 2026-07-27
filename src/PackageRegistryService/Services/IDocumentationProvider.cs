using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

public interface IDocumentationProvider
{
    DocumentationPage? GetPage(string document);
}
