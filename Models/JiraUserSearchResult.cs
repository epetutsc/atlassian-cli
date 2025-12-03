using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a search for Jira users.
/// </summary>
public class JiraUserSearchResult
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}
