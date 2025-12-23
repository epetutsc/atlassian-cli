using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a diff response from BitBucket.
/// </summary>
public class BitBucketDiffResponse
{
    [JsonPropertyName("fromHash")]
    public string? FromHash { get; set; }

    [JsonPropertyName("toHash")]
    public string? ToHash { get; set; }

    [JsonPropertyName("contextLines")]
    public int ContextLines { get; set; }

    [JsonPropertyName("whitespace")]
    public string? Whitespace { get; set; }

    [JsonPropertyName("diffs")]
    public List<BitBucketDiff>? Diffs { get; set; }

    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }
}

/// <summary>
/// Represents a single file diff.
/// </summary>
public class BitBucketDiff
{
    [JsonPropertyName("source")]
    public BitBucketDiffPath? Source { get; set; }

    [JsonPropertyName("destination")]
    public BitBucketDiffPath? Destination { get; set; }

    [JsonPropertyName("hunks")]
    public List<BitBucketDiffHunk>? Hunks { get; set; }

    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }
}

/// <summary>
/// Represents a file path in a diff.
/// </summary>
public class BitBucketDiffPath
{
    [JsonPropertyName("toString")]
    public new string? ToString { get; set; }

    [JsonPropertyName("parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("extension")]
    public string? Extension { get; set; }
}

/// <summary>
/// Represents a hunk in a diff (a section of changes).
/// </summary>
public class BitBucketDiffHunk
{
    [JsonPropertyName("sourceLine")]
    public int SourceLine { get; set; }

    [JsonPropertyName("sourceSpan")]
    public int SourceSpan { get; set; }

    [JsonPropertyName("destinationLine")]
    public int DestinationLine { get; set; }

    [JsonPropertyName("destinationSpan")]
    public int DestinationSpan { get; set; }

    [JsonPropertyName("segments")]
    public List<BitBucketDiffSegment>? Segments { get; set; }

    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }
}

/// <summary>
/// Represents a segment in a diff hunk.
/// </summary>
public class BitBucketDiffSegment
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("lines")]
    public List<BitBucketDiffLine>? Lines { get; set; }

    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }
}

/// <summary>
/// Represents a line in a diff segment.
/// </summary>
public class BitBucketDiffLine
{
    [JsonPropertyName("source")]
    public int Source { get; set; }

    [JsonPropertyName("destination")]
    public int Destination { get; set; }

    [JsonPropertyName("line")]
    public string? Line { get; set; }

    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }
}
