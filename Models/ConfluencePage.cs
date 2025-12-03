using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Confluence page with all properties.
/// </summary>
public class ConfluencePage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "page";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "current";

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("space")]
    public SpaceReference? Space { get; set; }

    [JsonPropertyName("body")]
    public PageBody? Body { get; set; }

    [JsonPropertyName("version")]
    public PageVersion? Version { get; set; }

    [JsonPropertyName("_links")]
    public PageLinks? Links { get; set; }
}
