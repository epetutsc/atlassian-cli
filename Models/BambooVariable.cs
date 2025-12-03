using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Bamboo plan variable.
/// </summary>
public class BambooVariable
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
