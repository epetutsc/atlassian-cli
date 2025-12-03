using CommandLine;

namespace AtlassianCli.Commands;

// ==================== Confluence Commands ====================

/// <summary>
/// Options for the create-page command.
/// </summary>
[Verb("create-page", HelpText = "Create a new page in Confluence.")]
public class CreatePageOptions
{
    [Option('s', "space", Required = true, HelpText = "The space key where the page will be created (e.g., 'MYSPACE').")]
    public string SpaceKey { get; set; } = string.Empty;

    [Option('t', "title", Required = true, HelpText = "The title of the new page.")]
    public string Title { get; set; } = string.Empty;

    [Option('b', "body", HelpText = "The body content in Confluence storage format (XHTML) or plain text. Either --body or --file is required.")]
    public string? Body { get; set; }

    [Option("file", HelpText = "Path to a UTF-8 encoded file containing the body content. Either --body or --file is required.")]
    public string? FilePath { get; set; }
}

/// <summary>
/// Options for the get-page command.
/// </summary>
[Verb("get-page", HelpText = "Retrieve a page from Confluence by ID or by title and space.")]
public class GetPageOptions
{
    [Option('i', "id", SetName = "byId", HelpText = "The ID of the page to retrieve.")]
    public string? PageId { get; set; }

    [Option('s', "space", SetName = "byTitle", HelpText = "The space key (required when using --title).")]
    public string? SpaceKey { get; set; }

    [Option('t', "title", SetName = "byTitle", HelpText = "The title of the page to retrieve (requires --space).")]
    public string? Title { get; set; }

    [Option('f', "format", Default = "storage", HelpText = "Output format: 'storage' (raw XHTML) or 'view' (rendered HTML).")]
    public string Format { get; set; } = "storage";
}

/// <summary>
/// Options for the update-page command.
/// </summary>
[Verb("update-page", HelpText = "Update an existing page in Confluence.")]
public class UpdatePageOptions
{
    [Option('i', "id", SetName = "byId", HelpText = "The ID of the page to update.")]
    public string? PageId { get; set; }

    [Option('s', "space", SetName = "byTitle", HelpText = "The space key (required when using --title).")]
    public string? SpaceKey { get; set; }

    [Option('t', "title", SetName = "byTitle", HelpText = "The title of the page to update (requires --space).")]
    public string? Title { get; set; }

    [Option('b', "body", HelpText = "The new body content in Confluence storage format (XHTML). Either --body or --file is required.")]
    public string? Body { get; set; }

    [Option("file", HelpText = "Path to a UTF-8 encoded file containing the body content. Either --body or --file is required.")]
    public string? FilePath { get; set; }

    [Option('a', "append", Default = false, HelpText = "Append content to existing page instead of replacing.")]
    public bool Append { get; set; }
}

// ==================== Jira Commands ====================

/// <summary>
/// Options for the get-issue command.
/// </summary>
[Verb("get-issue", HelpText = "Retrieve a Jira issue by key.")]
public class GetIssueOptions
{
    [Option('k', "key", Required = true, HelpText = "The issue key (e.g., PROJ-123).")]
    public string IssueKey { get; set; } = string.Empty;
}

/// <summary>
/// Options for the create-issue command.
/// </summary>
[Verb("create-issue", HelpText = "Create a new issue in Jira.")]
public class CreateIssueOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('s', "summary", Required = true, HelpText = "The issue summary/title.")]
    public string Summary { get; set; } = string.Empty;

    [Option('t', "type", Required = true, HelpText = "The issue type (e.g., Task, Bug, Story).")]
    public string IssueType { get; set; } = string.Empty;

    [Option('d', "description", Required = false, HelpText = "The issue description. Can be used with --description-file alternatively.")]
    public string? Description { get; set; }

    [Option("description-file", HelpText = "Path to a UTF-8 encoded file containing the description. Alternative to --description.")]
    public string? DescriptionFilePath { get; set; }
}

/// <summary>
/// Options for the add-comment command.
/// </summary>
[Verb("add-comment", HelpText = "Add a comment to a Jira issue.")]
public class AddCommentOptions
{
    [Option('k', "key", Required = true, HelpText = "The issue key (e.g., PROJ-123).")]
    public string IssueKey { get; set; } = string.Empty;

    [Option('b', "body", HelpText = "The comment body text. Either --body or --file is required.")]
    public string? Body { get; set; }

    [Option("file", HelpText = "Path to a UTF-8 encoded file containing the comment body. Either --body or --file is required.")]
    public string? FilePath { get; set; }
}

/// <summary>
/// Options for the change-status command.
/// </summary>
[Verb("change-status", HelpText = "Change the status of a Jira issue.")]
public class ChangeStatusOptions
{
    [Option('k', "key", Required = true, HelpText = "The issue key (e.g., PROJ-123).")]
    public string IssueKey { get; set; } = string.Empty;

    [Option('s', "status", Required = true, HelpText = "The target status name (e.g., 'In Progress', 'Done').")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Options for the assign-user command.
/// </summary>
[Verb("assign-user", HelpText = "Assign a user to a Jira issue.")]
public class AssignUserOptions
{
    [Option('k', "key", Required = true, HelpText = "The issue key (e.g., PROJ-123).")]
    public string IssueKey { get; set; } = string.Empty;

    [Option('u', "user", Required = true, HelpText = "The username or display name to assign.")]
    public string User { get; set; } = string.Empty;
}

/// <summary>
/// Options for the update-issue command.
/// </summary>
[Verb("update-issue", HelpText = "Update a Jira issue's description.")]
public class UpdateIssueOptions
{
    [Option('k', "key", Required = true, HelpText = "The issue key (e.g., PROJ-123).")]
    public string IssueKey { get; set; } = string.Empty;

    [Option('d', "description", HelpText = "The new description for the issue. Either --description or --file is required.")]
    public string? Description { get; set; }

    [Option("file", HelpText = "Path to a UTF-8 encoded file containing the description. Either --description or --file is required.")]
    public string? FilePath { get; set; }
}

// ==================== Bitbucket Commands ====================

/// <summary>
/// Options for the get-repo command.
/// </summary>
[Verb("get-repo", HelpText = "Retrieve a Bitbucket repository by project key and slug.")]
public class GetRepoOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;
}

/// <summary>
/// Options for the list-repos command.
/// </summary>
[Verb("list-repos", HelpText = "List repositories in a Bitbucket project.")]
public class ListReposOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('l', "limit", Default = 25, HelpText = "Maximum number of results to return.")]
    public int Limit { get; set; } = 25;
}

/// <summary>
/// Options for the list-branches command.
/// </summary>
[Verb("list-branches", HelpText = "List branches in a Bitbucket repository.")]
public class ListBranchesOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('l', "limit", Default = 25, HelpText = "Maximum number of results to return.")]
    public int Limit { get; set; } = 25;
}

/// <summary>
/// Options for the list-commits command.
/// </summary>
[Verb("list-commits", HelpText = "List commits in a Bitbucket repository.")]
public class ListCommitsOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('b', "branch", HelpText = "Filter commits by branch name.")]
    public string? Branch { get; set; }

    [Option('l', "limit", Default = 25, HelpText = "Maximum number of results to return.")]
    public int Limit { get; set; } = 25;
}

/// <summary>
/// Options for the get-commit command.
/// </summary>
[Verb("get-commit", HelpText = "Get a specific commit by ID.")]
public class GetCommitOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('c', "commit", Required = true, HelpText = "The commit ID (hash).")]
    public string CommitId { get; set; } = string.Empty;
}

/// <summary>
/// Options for the list-pull-requests command.
/// </summary>
[Verb("list-pull-requests", HelpText = "List pull requests in a Bitbucket repository.")]
public class ListPullRequestsOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('s', "state", Default = "OPEN", HelpText = "Filter by state (OPEN, MERGED, DECLINED, ALL).")]
    public string State { get; set; } = "OPEN";

    [Option('l', "limit", Default = 25, HelpText = "Maximum number of results to return.")]
    public int Limit { get; set; } = 25;
}

/// <summary>
/// Options for the get-pull-request command.
/// </summary>
[Verb("get-pull-request", HelpText = "Get a specific pull request by ID.")]
public class GetPullRequestOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('i', "id", Required = true, HelpText = "The pull request ID.")]
    public int PullRequestId { get; set; }
}

/// <summary>
/// Options for the get-project command.
/// </summary>
[Verb("get-project", HelpText = "Get a Bitbucket project by key.")]
public class GetProjectOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;
}

/// <summary>
/// Options for the list-projects command.
/// </summary>
[Verb("list-projects", HelpText = "List all Bitbucket projects.")]
public class ListProjectsOptions
{
    [Option('l', "limit", Default = 25, HelpText = "Maximum number of results to return.")]
    public int Limit { get; set; } = 25;
}

/// <summary>
/// Options for the get-webhooks command.
/// </summary>
[Verb("get-webhooks", HelpText = "Get webhooks configured for a repository.")]
public class GetWebhooksOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;
}

/// <summary>
/// Options for the get-branch-restrictions command.
/// </summary>
[Verb("get-branch-restrictions", HelpText = "Get branch restrictions for a repository.")]
public class GetBranchRestrictionsOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;
}

/// <summary>
/// Options for the list-pipelines command.
/// </summary>
[Verb("list-pipelines", HelpText = "List pipelines (runs) for a repository (Bitbucket Cloud only).")]
public class ListPipelinesOptions
{
    [Option('p', "project", Required = true, HelpText = "The workspace/project key.")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('l', "limit", Default = 25, HelpText = "Maximum number of results to return.")]
    public int Limit { get; set; } = 25;
}

/// <summary>
/// Options for the get-pipeline command.
/// </summary>
[Verb("get-pipeline", HelpText = "Get a specific pipeline by UUID (Bitbucket Cloud only).")]
public class GetPipelineOptions
{
    [Option('p', "project", Required = true, HelpText = "The workspace/project key.")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('u', "uuid", Required = true, HelpText = "The pipeline UUID.")]
    public string PipelineUuid { get; set; } = string.Empty;
}

/// <summary>
/// Options for the trigger-pipeline command.
/// </summary>
[Verb("trigger-pipeline", HelpText = "Trigger a pipeline run (Bitbucket Cloud only).")]
public class TriggerPipelineOptions
{
    [Option('p', "project", Required = true, HelpText = "The workspace/project key.")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('b', "branch", Required = true, HelpText = "The branch name to run the pipeline on.")]
    public string Branch { get; set; } = string.Empty;

    [Option('c', "custom", HelpText = "Custom pipeline name to run (optional).")]
    public string? CustomPipeline { get; set; }
}

/// <summary>
/// Options for the stop-pipeline command.
/// </summary>
[Verb("stop-pipeline", HelpText = "Stop a running pipeline (Bitbucket Cloud only).")]
public class StopPipelineOptions
{
    [Option('p', "project", Required = true, HelpText = "The workspace/project key.")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('u', "uuid", Required = true, HelpText = "The pipeline UUID to stop.")]
    public string PipelineUuid { get; set; } = string.Empty;
}

/// <summary>
/// Options for the get-build-status command.
/// </summary>
[Verb("get-build-status", HelpText = "Get build statuses for a commit.")]
public class GetBuildStatusOptions
{
    [Option('p', "project", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "The repository slug (e.g., my-repo).")]
    public string RepoSlug { get; set; } = string.Empty;

    [Option('c', "commit", Required = true, HelpText = "The commit ID (hash).")]
    public string CommitId { get; set; } = string.Empty;
}
