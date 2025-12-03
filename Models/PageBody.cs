using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents the body content of a Confluence page.
/// </summary>
public class PageBody
{
    [JsonPropertyName("storage")]
    public StorageContent? Storage { get; set; }

    [JsonPropertyName("view")]
    public ViewContent? View { get; set; }
}
