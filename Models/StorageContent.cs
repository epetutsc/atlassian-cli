using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents content in storage format.
/// </summary>
public class StorageContent
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("representation")]
    public string Representation { get; set; } = "storage";
}
