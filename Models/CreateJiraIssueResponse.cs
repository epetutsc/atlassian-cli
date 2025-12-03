using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Response from creating a Jira issue.
/// </summary>
public class CreateJiraIssueResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("self")]
    public string? Self { get; set; }
}
