using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Container for Bamboo build results list.
/// </summary>
public class BambooBuildResultsList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("result")]
    public List<BambooBuildResult> Result { get; set; } = new();
}
