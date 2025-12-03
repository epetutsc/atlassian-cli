using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents search results from Confluence API.
/// </summary>
public class SearchResults
{
    [JsonPropertyName("results")]
    public List<ConfluencePage> Results { get; set; } = new();

    [JsonPropertyName("start")]
    public int Start { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}
