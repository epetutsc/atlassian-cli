using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents the request body for creating a new page.
/// </summary>
public class CreatePageRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "page";

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("space")]
    public SpaceReference Space { get; set; } = new();

    [JsonPropertyName("body")]
    public PageBody Body { get; set; } = new();
}
