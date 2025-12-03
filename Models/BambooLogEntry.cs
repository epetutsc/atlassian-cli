using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Bamboo log entry.
/// </summary>
public class BambooLogEntry
{
    [JsonPropertyName("log")]
    public string? Log { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("unstyledLog")]
    public string? UnstyledLog { get; set; }

    [JsonPropertyName("formattedDate")]
    public string? FormattedDate { get; set; }
}
