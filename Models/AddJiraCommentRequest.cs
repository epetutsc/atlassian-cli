using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Request body for adding a comment to a Jira issue.
/// </summary>
public class AddJiraCommentRequest
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}
