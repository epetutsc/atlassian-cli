using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents rendered view content.
/// </summary>
public class ViewContent
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("representation")]
    public string Representation { get; set; } = "view";
}
