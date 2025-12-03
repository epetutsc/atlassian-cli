using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents the request body for updating an existing page.
/// </summary>
public class UpdatePageRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "page";

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public PageBody Body { get; set; } = new();

    [JsonPropertyName("version")]
    public PageVersion Version { get; set; } = new();
}
