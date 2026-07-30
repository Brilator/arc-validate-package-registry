namespace PackageRegistryService.Models;

public sealed class Author
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Affiliation { get; set; } = "";
    public string AffiliationLink { get; set; } = "";
}
