using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents an error response from Confluence API.
/// </summary>
public class ConfluenceError
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}
