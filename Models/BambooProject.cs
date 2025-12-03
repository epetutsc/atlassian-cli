using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Bamboo project.
/// </summary>
public class BambooProject
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("link")]
    public BambooLink? Link { get; set; }

    [JsonPropertyName("plans")]
    public BambooPlansList? Plans { get; set; }
}
