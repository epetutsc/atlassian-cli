using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Jira transition.
/// </summary>
public class JiraTransition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public JiraStatus? To { get; set; }
}
