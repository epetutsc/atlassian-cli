using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Container for Bamboo plans list.
/// </summary>
public class BambooPlansList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("plan")]
    public List<BambooPlan> Plan { get; set; } = new();
}
