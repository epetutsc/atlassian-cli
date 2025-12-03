using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Container for Jira comments.
/// </summary>
public class JiraCommentContainer
{
    [JsonPropertyName("comments")]
    public List<JiraComment> Comments { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}
