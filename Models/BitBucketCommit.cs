using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a commit in BitBucket.
/// </summary>
public class BitBucketCommit
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("displayId")]
    public string? DisplayId { get; set; }

    [JsonPropertyName("author")]
    public BitBucketCommitAuthor? Author { get; set; }

    [JsonPropertyName("authorTimestamp")]
    public long AuthorTimestamp { get; set; }

    [JsonPropertyName("committer")]
    public BitBucketCommitAuthor? Committer { get; set; }

    [JsonPropertyName("committerTimestamp")]
    public long CommitterTimestamp { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("parents")]
    public List<BitBucketCommitParent>? Parents { get; set; }
}

/// <summary>
/// Represents the author or committer of a commit.
/// </summary>
public class BitBucketCommitAuthor
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }
}

/// <summary>
/// Represents a parent commit.
/// </summary>
public class BitBucketCommitParent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("displayId")]
    public string? DisplayId { get; set; }
}

/// <summary>
/// Represents paginated commits response.
/// </summary>
public class BitBucketCommitsResponse
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("isLastPage")]
    public bool IsLastPage { get; set; }

    [JsonPropertyName("values")]
    public List<BitBucketCommit>? Values { get; set; }

    [JsonPropertyName("start")]
    public int Start { get; set; }

    [JsonPropertyName("nextPageStart")]
    public int? NextPageStart { get; set; }
}
