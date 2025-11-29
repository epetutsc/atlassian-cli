using System.Text.Json.Serialization;

namespace ConfluenceCli.Models;

/// <summary>
/// Represents a Confluence space reference.
/// </summary>
public class SpaceReference
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Represents the body content of a Confluence page.
/// </summary>
public class PageBody
{
    [JsonPropertyName("storage")]
    public StorageContent? Storage { get; set; }

    [JsonPropertyName("view")]
    public ViewContent? View { get; set; }
}

/// <summary>
/// Represents content in storage format.
/// </summary>
public class StorageContent
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("representation")]
    public string Representation { get; set; } = "storage";
}

/// <summary>
/// Represents rendered view content.
/// </summary>
public class ViewContent
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("representation")]
    public string Representation { get; set; } = "view";
}

/// <summary>
/// Represents version information for a Confluence page.
/// </summary>
public class PageVersion
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// Represents a Confluence page with all properties.
/// </summary>
public class ConfluencePage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "page";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "current";

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("space")]
    public SpaceReference? Space { get; set; }

    [JsonPropertyName("body")]
    public PageBody? Body { get; set; }

    [JsonPropertyName("version")]
    public PageVersion? Version { get; set; }

    [JsonPropertyName("_links")]
    public PageLinks? Links { get; set; }
}

/// <summary>
/// Represents links associated with a Confluence page.
/// </summary>
public class PageLinks
{
    [JsonPropertyName("webui")]
    public string? WebUi { get; set; }

    [JsonPropertyName("self")]
    public string? Self { get; set; }
}

/// <summary>
/// Represents the request body for creating a new page.
/// </summary>
public class CreatePageRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "page";

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("space")]
    public SpaceReference Space { get; set; } = new();

    [JsonPropertyName("body")]
    public PageBody Body { get; set; } = new();
}

/// <summary>
/// Represents the request body for updating an existing page.
/// </summary>
public class UpdatePageRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "page";

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public PageBody Body { get; set; } = new();

    [JsonPropertyName("version")]
    public PageVersion Version { get; set; } = new();
}

/// <summary>
/// Represents search results from Confluence API.
/// </summary>
public class SearchResults
{
    [JsonPropertyName("results")]
    public List<ConfluencePage> Results { get; set; } = new();

    [JsonPropertyName("start")]
    public int Start { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}

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
