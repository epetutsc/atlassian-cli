using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Container for Bamboo branches list.
/// </summary>
public class BambooBranchesList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("branch")]
    public List<BambooBranch> Branch { get; set; } = new();
}
