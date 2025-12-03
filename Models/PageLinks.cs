using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents links associated with a Confluence page.
/// </summary>
public class PageLinks
{
    [JsonPropertyName("webui")]
    public string? WebUi { get; set; }

    [JsonPropertyName("self")]
    public string? Self { get; set; }
}
