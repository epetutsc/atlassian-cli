using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Issue type name reference for creating issues.
/// </summary>
public class IssueTypeName
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
