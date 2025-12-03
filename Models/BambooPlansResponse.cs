using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents the response containing a list of Bamboo plans.
/// </summary>
public class BambooPlansResponse
{
    [JsonPropertyName("plans")]
    public BambooPlansList Plans { get; set; } = new();
}
