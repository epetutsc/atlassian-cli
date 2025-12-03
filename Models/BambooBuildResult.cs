using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Bamboo build result.
/// </summary>
public class BambooBuildResult
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("buildNumber")]
    public int BuildNumber { get; set; }

    [JsonPropertyName("buildResultKey")]
    public string? BuildResultKey { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("buildState")]
    public string? BuildState { get; set; }

    [JsonPropertyName("lifeCycleState")]
    public string? LifeCycleState { get; set; }

    [JsonPropertyName("successful")]
    public bool Successful { get; set; }

    [JsonPropertyName("finished")]
    public bool Finished { get; set; }

    [JsonPropertyName("buildReason")]
    public string? BuildReason { get; set; }

    [JsonPropertyName("reasonSummary")]
    public string? ReasonSummary { get; set; }

    [JsonPropertyName("plan")]
    public BambooPlan? Plan { get; set; }

    [JsonPropertyName("planName")]
    public string? PlanName { get; set; }

    [JsonPropertyName("projectName")]
    public string? ProjectName { get; set; }

    [JsonPropertyName("buildStartedTime")]
    public string? BuildStartedTime { get; set; }

    [JsonPropertyName("buildCompletedTime")]
    public string? BuildCompletedTime { get; set; }

    [JsonPropertyName("buildDuration")]
    public long? BuildDuration { get; set; }

    [JsonPropertyName("buildDurationDescription")]
    public string? BuildDurationDescription { get; set; }

    [JsonPropertyName("buildDurationInSeconds")]
    public long? BuildDurationInSeconds { get; set; }

    [JsonPropertyName("buildRelativeTime")]
    public string? BuildRelativeTime { get; set; }

    [JsonPropertyName("link")]
    public BambooLink? Link { get; set; }

    [JsonPropertyName("stages")]
    public BambooStageResultsList? Stages { get; set; }

    [JsonPropertyName("changes")]
    public BambooChangesList? Changes { get; set; }

    [JsonPropertyName("successfulTestCount")]
    public int SuccessfulTestCount { get; set; }

    [JsonPropertyName("failedTestCount")]
    public int FailedTestCount { get; set; }

    [JsonPropertyName("quarantinedTestCount")]
    public int QuarantinedTestCount { get; set; }

    [JsonPropertyName("skippedTestCount")]
    public int SkippedTestCount { get; set; }

    [JsonPropertyName("logEntries")]
    public BambooLogEntriesList? LogEntries { get; set; }
}
