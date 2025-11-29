using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Jira issue.
/// </summary>
public class JiraIssue
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("self")]
    public string? Self { get; set; }

    [JsonPropertyName("fields")]
    public JiraIssueFields Fields { get; set; } = new();
}

/// <summary>
/// Represents the fields of a Jira issue.
/// </summary>
public class JiraIssueFields
{
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("issuetype")]
    public JiraIssueType? IssueType { get; set; }

    [JsonPropertyName("project")]
    public JiraProject? Project { get; set; }

    [JsonPropertyName("status")]
    public JiraStatus? Status { get; set; }

    [JsonPropertyName("assignee")]
    public JiraUser? Assignee { get; set; }

    [JsonPropertyName("reporter")]
    public JiraUser? Reporter { get; set; }

    [JsonPropertyName("priority")]
    public JiraPriority? Priority { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }

    [JsonPropertyName("updated")]
    public string? Updated { get; set; }

    [JsonPropertyName("comment")]
    public JiraCommentContainer? Comment { get; set; }
}

/// <summary>
/// Represents a Jira issue type.
/// </summary>
public class JiraIssueType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Represents a Jira project.
/// </summary>
public class JiraProject
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Jira issue status.
/// </summary>
public class JiraStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("statusCategory")]
    public JiraStatusCategory? StatusCategory { get; set; }
}

/// <summary>
/// Represents a Jira status category.
/// </summary>
public class JiraStatusCategory
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Jira user.
/// </summary>
public class JiraUser
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}

/// <summary>
/// Represents a Jira priority.
/// </summary>
public class JiraPriority
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Container for Jira comments.
/// </summary>
public class JiraCommentContainer
{
    [JsonPropertyName("comments")]
    public List<JiraComment> Comments { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

/// <summary>
/// Represents a Jira comment.
/// </summary>
public class JiraComment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public JiraUser? Author { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }

    [JsonPropertyName("updated")]
    public string? Updated { get; set; }
}

/// <summary>
/// Request body for creating a Jira issue.
/// </summary>
public class CreateJiraIssueRequest
{
    [JsonPropertyName("fields")]
    public CreateJiraIssueFields Fields { get; set; } = new();
}

/// <summary>
/// Fields for creating a Jira issue.
/// </summary>
public class CreateJiraIssueFields
{
    [JsonPropertyName("project")]
    public ProjectKey Project { get; set; } = new();

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("issuetype")]
    public IssueTypeName IssueType { get; set; } = new();
}

/// <summary>
/// Project key reference for creating issues.
/// </summary>
public class ProjectKey
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Issue type name reference for creating issues.
/// </summary>
public class IssueTypeName
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Response from creating a Jira issue.
/// </summary>
public class CreateJiraIssueResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("self")]
    public string? Self { get; set; }
}

/// <summary>
/// Request body for adding a comment to a Jira issue.
/// </summary>
public class AddJiraCommentRequest
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Jira transition.
/// </summary>
public class JiraTransition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public JiraStatus? To { get; set; }
}

/// <summary>
/// Response containing available transitions.
/// </summary>
public class JiraTransitionsResponse
{
    [JsonPropertyName("transitions")]
    public List<JiraTransition> Transitions { get; set; } = new();
}

/// <summary>
/// Request body for transitioning a Jira issue.
/// </summary>
public class TransitionJiraIssueRequest
{
    [JsonPropertyName("transition")]
    public TransitionId Transition { get; set; } = new();
}

/// <summary>
/// Transition ID reference.
/// </summary>
public class TransitionId
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Request body for assigning a user to a Jira issue.
/// </summary>
public class AssignJiraIssueRequest
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Request body for updating a Jira issue.
/// </summary>
public class UpdateJiraIssueRequest
{
    [JsonPropertyName("fields")]
    public UpdateJiraIssueFields Fields { get; set; } = new();
}

/// <summary>
/// Fields for updating a Jira issue.
/// </summary>
public class UpdateJiraIssueFields
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
}

/// <summary>
/// Represents a search for Jira users.
/// </summary>
public class JiraUserSearchResult
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}
