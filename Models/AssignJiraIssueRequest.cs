using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Request body for assigning a user to a Jira issue.
/// </summary>
public class AssignJiraIssueRequest
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
