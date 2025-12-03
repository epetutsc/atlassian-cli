using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Request body for transitioning a Jira issue.
/// </summary>
public class TransitionJiraIssueRequest
{
    [JsonPropertyName("transition")]
    public TransitionId Transition { get; set; } = new();
}
