using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Request body for creating a Jira issue.
/// </summary>
public class CreateJiraIssueRequest
{
    [JsonPropertyName("fields")]
    public CreateJiraIssueFields Fields { get; set; } = new();
}
