using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Container for Bamboo log entries list.
/// </summary>
public class BambooLogEntriesList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("logEntry")]
    public List<BambooLogEntry> LogEntry { get; set; } = new();
}
