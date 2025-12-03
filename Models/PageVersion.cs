using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents version information for a Confluence page.
/// </summary>
public class PageVersion
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
