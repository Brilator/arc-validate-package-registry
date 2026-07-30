using System.Text.Json.Serialization;

namespace PackageRegistryService.Models;

public sealed class CommandInputParameter
{
    [JsonPropertyName("id"), JsonRequired]
    public string Id { get; set; } = "";

    [JsonPropertyName("type"), JsonRequired]
    public CommandInputType Type { get; set; } = new();

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("doc")]
    public string Doc { get; set; } = "";

    [JsonPropertyName("inputBinding"), JsonRequired]
    public CommandInputBinding InputBinding { get; set; } = new();
}
