using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a BitBucket pull request.
/// </summary>
public class BitBucketPullRequest
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("open")]
    public bool Open { get; set; }

    [JsonPropertyName("closed")]
    public bool Closed { get; set; }

    [JsonPropertyName("createdDate")]
    public long CreatedDate { get; set; }

    [JsonPropertyName("updatedDate")]
    public long UpdatedDate { get; set; }

    [JsonPropertyName("fromRef")]
    public BitBucketRef? FromRef { get; set; }

    [JsonPropertyName("toRef")]
    public BitBucketRef? ToRef { get; set; }

    [JsonPropertyName("author")]
    public BitBucketParticipant? Author { get; set; }

    [JsonPropertyName("reviewers")]
    public List<BitBucketReviewer>? Reviewers { get; set; }

    [JsonPropertyName("participants")]
    public List<BitBucketParticipant>? Participants { get; set; }

    [JsonPropertyName("links")]
    public BitBucketLinks? Links { get; set; }
}

/// <summary>
/// Represents a reference to a branch in BitBucket.
/// </summary>
public class BitBucketRef
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("displayId")]
    public string? DisplayId { get; set; }

    [JsonPropertyName("latestCommit")]
    public string? LatestCommit { get; set; }

    [JsonPropertyName("repository")]
    public BitBucketRepository? Repository { get; set; }
}

/// <summary>
/// Represents a BitBucket repository.
/// </summary>
public class BitBucketRepository
{
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("project")]
    public BitBucketProject? Project { get; set; }
}

/// <summary>
/// Represents a BitBucket project.
/// </summary>
public class BitBucketProject
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Represents a BitBucket user.
/// </summary>
public class BitBucketUser
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

/// <summary>
/// Represents a reviewer for a pull request.
/// </summary>
public class BitBucketReviewer
{
    [JsonPropertyName("user")]
    public BitBucketUser? User { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("approved")]
    public bool Approved { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// Represents a participant in a pull request.
/// </summary>
public class BitBucketParticipant
{
    [JsonPropertyName("user")]
    public BitBucketUser? User { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("approved")]
    public bool Approved { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// Represents links in BitBucket API response.
/// </summary>
public class BitBucketLinks
{
    [JsonPropertyName("self")]
    public List<BitBucketLink>? Self { get; set; }
}

/// <summary>
/// Represents a single link.
/// </summary>
public class BitBucketLink
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
}
