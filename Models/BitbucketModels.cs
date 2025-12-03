using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Bitbucket repository.
/// </summary>
public class BitbucketRepository
{
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("scmId")]
    public string ScmId { get; set; } = "git";

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("forkable")]
    public bool Forkable { get; set; }

    [JsonPropertyName("public")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("project")]
    public BitbucketProject? Project { get; set; }

    [JsonPropertyName("links")]
    public BitbucketLinks? Links { get; set; }
}

/// <summary>
/// Represents a Bitbucket project.
/// </summary>
public class BitbucketProject
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("public")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("links")]
    public BitbucketLinks? Links { get; set; }
}

/// <summary>
/// Represents a Bitbucket branch.
/// </summary>
public class BitbucketBranch
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayId")]
    public string DisplayId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "BRANCH";

    [JsonPropertyName("latestCommit")]
    public string? LatestCommit { get; set; }

    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }
}

/// <summary>
/// Represents a Bitbucket commit.
/// </summary>
public class BitbucketCommit
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayId")]
    public string DisplayId { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public BitbucketAuthor? Author { get; set; }

    [JsonPropertyName("authorTimestamp")]
    public long AuthorTimestamp { get; set; }

    [JsonPropertyName("committer")]
    public BitbucketAuthor? Committer { get; set; }

    [JsonPropertyName("committerTimestamp")]
    public long CommitterTimestamp { get; set; }

    [JsonPropertyName("parents")]
    public List<BitbucketParentCommit>? Parents { get; set; }
}

/// <summary>
/// Represents a parent commit reference.
/// </summary>
public class BitbucketParentCommit
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayId")]
    public string DisplayId { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Bitbucket author/committer.
/// </summary>
public class BitbucketAuthor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }
}

/// <summary>
/// Represents a Bitbucket pull request.
/// </summary>
public class BitbucketPullRequest
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = "OPEN";

    [JsonPropertyName("open")]
    public bool Open { get; set; }

    [JsonPropertyName("closed")]
    public bool Closed { get; set; }

    [JsonPropertyName("createdDate")]
    public long CreatedDate { get; set; }

    [JsonPropertyName("updatedDate")]
    public long UpdatedDate { get; set; }

    [JsonPropertyName("fromRef")]
    public BitbucketRef? FromRef { get; set; }

    [JsonPropertyName("toRef")]
    public BitbucketRef? ToRef { get; set; }

    [JsonPropertyName("author")]
    public BitbucketPullRequestParticipant? Author { get; set; }

    [JsonPropertyName("reviewers")]
    public List<BitbucketPullRequestParticipant>? Reviewers { get; set; }

    [JsonPropertyName("links")]
    public BitbucketLinks? Links { get; set; }
}

/// <summary>
/// Represents a branch/tag reference.
/// </summary>
public class BitbucketRef
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayId")]
    public string DisplayId { get; set; } = string.Empty;

    [JsonPropertyName("latestCommit")]
    public string? LatestCommit { get; set; }

    [JsonPropertyName("repository")]
    public BitbucketRepository? Repository { get; set; }
}

/// <summary>
/// Represents a pull request participant (author or reviewer).
/// </summary>
public class BitbucketPullRequestParticipant
{
    [JsonPropertyName("user")]
    public BitbucketUser? User { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("approved")]
    public bool Approved { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// Represents a Bitbucket user.
/// </summary>
public class BitbucketUser
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "NORMAL";

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; } = true;
}

/// <summary>
/// Represents links associated with Bitbucket resources.
/// </summary>
public class BitbucketLinks
{
    [JsonPropertyName("self")]
    public List<BitbucketLink>? Self { get; set; }

    [JsonPropertyName("clone")]
    public List<BitbucketCloneLink>? Clone { get; set; }
}

/// <summary>
/// Represents a single link.
/// </summary>
public class BitbucketLink
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
}

/// <summary>
/// Represents a clone link.
/// </summary>
public class BitbucketCloneLink
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents a paginated response from Bitbucket API.
/// </summary>
/// <typeparam name="T">The type of values in the response.</typeparam>
public class BitbucketPagedResponse<T>
{
    [JsonPropertyName("values")]
    public List<T> Values { get; set; } = new();

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("start")]
    public int Start { get; set; }

    [JsonPropertyName("isLastPage")]
    public bool IsLastPage { get; set; }

    [JsonPropertyName("nextPageStart")]
    public int? NextPageStart { get; set; }
}

/// <summary>
/// Represents a Bitbucket build status (pipeline status).
/// </summary>
public class BitbucketBuildStatus
{
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("dateAdded")]
    public long DateAdded { get; set; }
}

/// <summary>
/// Represents repository settings/configuration.
/// </summary>
public class BitbucketRepositorySettings
{
    [JsonPropertyName("requireAllReviewersApprove")]
    public bool RequireAllReviewersApprove { get; set; }

    [JsonPropertyName("requiredApprovals")]
    public int RequiredApprovals { get; set; }

    [JsonPropertyName("requiredAllTasksComplete")]
    public bool RequiredAllTasksComplete { get; set; }

    [JsonPropertyName("requiredSuccessfulBuilds")]
    public int RequiredSuccessfulBuilds { get; set; }
}

/// <summary>
/// Represents branch restrictions/permissions.
/// </summary>
public class BitbucketBranchRestriction
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("matcher")]
    public BitbucketBranchMatcher? Matcher { get; set; }

    [JsonPropertyName("users")]
    public List<BitbucketUser>? Users { get; set; }

    [JsonPropertyName("groups")]
    public List<string>? Groups { get; set; }
}

/// <summary>
/// Represents a branch matcher for restrictions.
/// </summary>
public class BitbucketBranchMatcher
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayId")]
    public string DisplayId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public BitbucketMatcherType? Type { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}

/// <summary>
/// Represents a matcher type.
/// </summary>
public class BitbucketMatcherType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents a webhook configuration.
/// </summary>
public class BitbucketWebhook
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<string> Events { get; set; } = new();

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}

/// <summary>
/// Represents a pipeline trigger request for Bitbucket Pipelines.
/// </summary>
public class BitbucketPipelineTriggerRequest
{
    [JsonPropertyName("target")]
    public BitbucketPipelineTarget Target { get; set; } = new();

    [JsonPropertyName("variables")]
    public List<BitbucketPipelineVariable>? Variables { get; set; }
}

/// <summary>
/// Represents a pipeline target (branch/commit to run pipeline on).
/// </summary>
public class BitbucketPipelineTarget
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "pipeline_ref_target";

    [JsonPropertyName("ref_type")]
    public string RefType { get; set; } = "branch";

    [JsonPropertyName("ref_name")]
    public string RefName { get; set; } = string.Empty;

    [JsonPropertyName("selector")]
    public BitbucketPipelineSelector? Selector { get; set; }
}

/// <summary>
/// Represents a pipeline selector (custom pipeline selection).
/// </summary>
public class BitbucketPipelineSelector
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "custom";

    [JsonPropertyName("pattern")]
    public string Pattern { get; set; } = string.Empty;
}

/// <summary>
/// Represents a pipeline variable.
/// </summary>
public class BitbucketPipelineVariable
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("secured")]
    public bool Secured { get; set; }
}

/// <summary>
/// Represents a pipeline run/execution.
/// </summary>
public class BitbucketPipeline
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    [JsonPropertyName("build_number")]
    public int BuildNumber { get; set; }

    [JsonPropertyName("state")]
    public BitbucketPipelineState? State { get; set; }

    [JsonPropertyName("created_on")]
    public string? CreatedOn { get; set; }

    [JsonPropertyName("completed_on")]
    public string? CompletedOn { get; set; }

    [JsonPropertyName("target")]
    public BitbucketPipelineTargetInfo? Target { get; set; }

    [JsonPropertyName("trigger")]
    public BitbucketPipelineTrigger? Trigger { get; set; }

    [JsonPropertyName("duration_in_seconds")]
    public int? DurationInSeconds { get; set; }
}

/// <summary>
/// Represents the state of a pipeline.
/// </summary>
public class BitbucketPipelineState
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public BitbucketPipelineResult? Result { get; set; }
}

/// <summary>
/// Represents the result of a pipeline.
/// </summary>
public class BitbucketPipelineResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents target information for a pipeline.
/// </summary>
public class BitbucketPipelineTargetInfo
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("ref_type")]
    public string? RefType { get; set; }

    [JsonPropertyName("ref_name")]
    public string? RefName { get; set; }

    [JsonPropertyName("commit")]
    public BitbucketPipelineCommit? Commit { get; set; }
}

/// <summary>
/// Represents a commit in a pipeline context.
/// </summary>
public class BitbucketPipelineCommit
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "commit";
}

/// <summary>
/// Represents a pipeline trigger.
/// </summary>
public class BitbucketPipelineTrigger
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Represents pipeline configuration from bitbucket-pipelines.yml.
/// </summary>
public class BitbucketPipelineConfiguration
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("repository")]
    public BitbucketRepository? Repository { get; set; }
}
