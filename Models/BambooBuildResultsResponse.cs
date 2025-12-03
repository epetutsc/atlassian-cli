using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents the response containing build results.
/// </summary>
public class BambooBuildResultsResponse
{
    [JsonPropertyName("results")]
    public BambooBuildResultsList Results { get; set; } = new();
}
