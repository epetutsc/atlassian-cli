using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Project key reference for creating issues.
/// </summary>
public class ProjectKey
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
}
