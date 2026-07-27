using System.Text.Json.Serialization;

namespace PackageRegistryService.Models;

public sealed record ServiceVersionDocument(
    [property: JsonPropertyName("service")] ServiceIdentity Service,
    [property: JsonPropertyName("api")] ApiIdentity Api,
    [property: JsonPropertyName("build")] BuildIdentity Build,
    [property: JsonPropertyName("release")] ReleaseIdentity Release);

public sealed record ServiceIdentity(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version);

public sealed record ApiIdentity(
    [property: JsonPropertyName("versions")] string[] Versions);

public sealed record BuildIdentity(
    [property: JsonPropertyName("revision")] string Revision,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("created")] DateTimeOffset? Created);

public sealed record ReleaseIdentity(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("notesUrl")] string NotesUrl);
