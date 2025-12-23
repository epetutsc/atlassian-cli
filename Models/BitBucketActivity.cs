using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents an activity (comment or system event) on a pull request.
/// </summary>
public class BitBucketActivity
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("createdDate")]
    public long CreatedDate { get; set; }

    [JsonPropertyName("user")]
    public BitBucketUser? User { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("comment")]
    public BitBucketComment? Comment { get; set; }

    [JsonPropertyName("commentAnchor")]
    public BitBucketCommentAnchor? CommentAnchor { get; set; }
}

/// <summary>
/// Represents a comment on a pull request.
/// </summary>
public class BitBucketComment
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("author")]
    public BitBucketUser? Author { get; set; }

    [JsonPropertyName("createdDate")]
    public long CreatedDate { get; set; }

    [JsonPropertyName("updatedDate")]
    public long UpdatedDate { get; set; }

    [JsonPropertyName("comments")]
    public List<BitBucketComment>? Comments { get; set; }

    [JsonPropertyName("tasks")]
    public List<object>? Tasks { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

/// <summary>
/// Represents a comment anchor (location in diff).
/// </summary>
public class BitBucketCommentAnchor
{
    [JsonPropertyName("fromHash")]
    public string? FromHash { get; set; }

    [JsonPropertyName("toHash")]
    public string? ToHash { get; set; }

    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("lineType")]
    public string? LineType { get; set; }

    [JsonPropertyName("fileType")]
    public string? FileType { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("srcPath")]
    public string? SrcPath { get; set; }
}

/// <summary>
/// Represents paginated activities response.
/// </summary>
public class BitBucketActivitiesResponse
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("isLastPage")]
    public bool IsLastPage { get; set; }

    [JsonPropertyName("values")]
    public List<BitBucketActivity>? Values { get; set; }

    [JsonPropertyName("start")]
    public int Start { get; set; }

    [JsonPropertyName("nextPageStart")]
    public int? NextPageStart { get; set; }
}

/// <summary>
/// Request to add a comment to a pull request.
/// </summary>
public class AddBitBucketCommentRequest
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
