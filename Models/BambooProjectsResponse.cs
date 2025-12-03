using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents the response containing a list of Bamboo projects.
/// </summary>
public class BambooProjectsResponse
{
    [JsonPropertyName("projects")]
    public BambooProjectsList Projects { get; set; } = new();
}
