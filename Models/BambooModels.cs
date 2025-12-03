using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

// ==================== Project Models ====================

/// <summary>
/// Represents the response containing a list of Bamboo projects.
/// </summary>
public class BambooProjectsResponse
{
    [JsonPropertyName("projects")]
    public BambooProjectsList Projects { get; set; } = new();
}

/// <summary>
/// Container for Bamboo projects list.
/// </summary>
public class BambooProjectsList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("project")]
    public List<BambooProject> Project { get; set; } = new();
}

/// <summary>
/// Represents a Bamboo project.
/// </summary>
public class BambooProject
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("link")]
    public BambooLink? Link { get; set; }

    [JsonPropertyName("plans")]
    public BambooPlansList? Plans { get; set; }
}

// ==================== Plan Models ====================

/// <summary>
/// Represents the response containing a list of Bamboo plans.
/// </summary>
public class BambooPlansResponse
{
    [JsonPropertyName("plans")]
    public BambooPlansList Plans { get; set; } = new();
}

/// <summary>
/// Container for Bamboo plans list.
/// </summary>
public class BambooPlansList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("plan")]
    public List<BambooPlan> Plan { get; set; } = new();
}

/// <summary>
/// Represents a Bamboo plan.
/// </summary>
public class BambooPlan
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("shortKey")]
    public string? ShortKey { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("projectKey")]
    public string? ProjectKey { get; set; }

    [JsonPropertyName("projectName")]
    public string? ProjectName { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("buildName")]
    public string? BuildName { get; set; }

    [JsonPropertyName("averageBuildTimeInSeconds")]
    public double? AverageBuildTimeInSeconds { get; set; }

    [JsonPropertyName("link")]
    public BambooLink? Link { get; set; }

    [JsonPropertyName("isFavourite")]
    public bool IsFavourite { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("isBuilding")]
    public bool IsBuilding { get; set; }

    [JsonPropertyName("stages")]
    public BambooStagesList? Stages { get; set; }

    [JsonPropertyName("branches")]
    public BambooBranchesList? Branches { get; set; }

    [JsonPropertyName("variableContext")]
    public BambooVariableContext? VariableContext { get; set; }
}

// ==================== Branch Models ====================

/// <summary>
/// Container for Bamboo branches list.
/// </summary>
public class BambooBranchesList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("branch")]
    public List<BambooBranch> Branch { get; set; } = new();
}

/// <summary>
/// Represents a Bamboo branch.
/// </summary>
public class BambooBranch
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortKey")]
    public string? ShortKey { get; set; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("link")]
    public BambooLink? Link { get; set; }
}

// ==================== Stage Models ====================

/// <summary>
/// Container for Bamboo stages list.
/// </summary>
public class BambooStagesList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("stage")]
    public List<BambooStage> Stage { get; set; } = new();
}

/// <summary>
/// Represents a Bamboo stage.
/// </summary>
public class BambooStage
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

// ==================== Variable Models ====================

/// <summary>
/// Container for Bamboo variables.
/// </summary>
public class BambooVariableContext
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("variable")]
    public List<BambooVariable> Variable { get; set; } = new();
}

/// <summary>
/// Represents a Bamboo plan variable.
/// </summary>
public class BambooVariable
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

// ==================== Build Result Models ====================

/// <summary>
/// Represents the response containing build results.
/// </summary>
public class BambooBuildResultsResponse
{
    [JsonPropertyName("results")]
    public BambooBuildResultsList Results { get; set; } = new();
}

/// <summary>
/// Container for Bamboo build results list.
/// </summary>
public class BambooBuildResultsList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("result")]
    public List<BambooBuildResult> Result { get; set; } = new();
}

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
}

// ==================== Stage Result Models ====================

/// <summary>
/// Container for Bamboo stage results list.
/// </summary>
public class BambooStageResultsList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("stage")]
    public List<BambooStageResult> Stage { get; set; } = new();
}

/// <summary>
/// Represents a Bamboo stage result.
/// </summary>
public class BambooStageResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("finished")]
    public bool Finished { get; set; }

    [JsonPropertyName("successful")]
    public bool Successful { get; set; }
}

// ==================== Changes Models ====================

/// <summary>
/// Container for Bamboo changes list.
/// </summary>
public class BambooChangesList
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("start-index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("max-result")]
    public int MaxResult { get; set; }

    [JsonPropertyName("change")]
    public List<BambooChange> Change { get; set; } = new();
}

/// <summary>
/// Represents a code change associated with a build.
/// </summary>
public class BambooChange
{
    [JsonPropertyName("changesetId")]
    public string? ChangesetId { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("commitUrl")]
    public string? CommitUrl { get; set; }
}

// ==================== Queue Models ====================

/// <summary>
/// Response from queuing a build.
/// </summary>
public class BambooQueueResponse
{
    [JsonPropertyName("buildNumber")]
    public int BuildNumber { get; set; }

    [JsonPropertyName("buildResultKey")]
    public string? BuildResultKey { get; set; }

    [JsonPropertyName("planKey")]
    public string? PlanKey { get; set; }

    [JsonPropertyName("link")]
    public BambooLink? Link { get; set; }

    [JsonPropertyName("triggerReason")]
    public string? TriggerReason { get; set; }
}

// ==================== Common Models ====================

/// <summary>
/// Represents a Bamboo link.
/// </summary>
public class BambooLink
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    [JsonPropertyName("rel")]
    public string? Rel { get; set; }
}
