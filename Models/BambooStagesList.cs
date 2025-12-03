using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Container for Bamboo stages list.
/// </summary>
public class BambooStagesList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("stage")]
    public List<BambooStage> Stage { get; set; } = new();
}
