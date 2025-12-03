using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Jira comment.
/// </summary>
public class JiraComment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public JiraUser? Author { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }

    [JsonPropertyName("updated")]
    public string? Updated { get; set; }
}
