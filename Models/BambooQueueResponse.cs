using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Response from queuing a build.
/// </summary>
public class BambooQueueResponse
{
    [JsonPropertyName("buildNumber")]
    public int BuildNumber { get; set; }

    [JsonPropertyName("buildResultKey")]
    public string? BuildResultKey { get; set; }

    [JsonPropertyName("planKey")]
    public string? PlanKey { get; set; }

    [JsonPropertyName("link")]
    public BambooLink? Link { get; set; }

    [JsonPropertyName("triggerReason")]
    public string? TriggerReason { get; set; }
}
