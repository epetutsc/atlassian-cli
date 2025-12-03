using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Fields for creating a Jira issue.
/// </summary>
public class CreateJiraIssueFields
{
    [JsonPropertyName("project")]
    public ProjectKey Project { get; set; } = new();

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("issuetype")]
    public IssueTypeName IssueType { get; set; } = new();
}
