using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Request body for updating a Jira issue.
/// </summary>
public class UpdateJiraIssueRequest
{
    [JsonPropertyName("fields")]
    public UpdateJiraIssueFields Fields { get; set; } = new();
}
