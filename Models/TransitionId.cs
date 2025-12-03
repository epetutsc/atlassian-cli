using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Transition ID reference.
/// </summary>
public class TransitionId
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
