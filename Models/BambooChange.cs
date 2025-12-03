using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a code change associated with a build.
/// </summary>
public class BambooChange
{
    [JsonPropertyName("changesetId")]
    public string? ChangesetId { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("commitUrl")]
    public string? CommitUrl { get; set; }
}
