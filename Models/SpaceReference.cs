using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Confluence space reference.
/// </summary>
public class SpaceReference
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
}
