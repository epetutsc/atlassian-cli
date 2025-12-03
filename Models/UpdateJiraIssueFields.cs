using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Fields for updating a Jira issue.
/// </summary>
public class UpdateJiraIssueFields
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
}
