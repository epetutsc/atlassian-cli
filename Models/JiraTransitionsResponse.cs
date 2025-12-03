using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Response containing available transitions.
/// </summary>
public class JiraTransitionsResponse
{
    [JsonPropertyName("transitions")]
    public List<JiraTransition> Transitions { get; set; } = new();
}
