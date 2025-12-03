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

// ==================== Bamboo Commands ====================

/// <summary>
/// Options for the get-projects command.
/// </summary>
[Verb("get-projects", HelpText = "List all Bamboo projects.")]
public class GetBambooProjectsOptions
{
}

/// <summary>
/// Options for the get-project command.
/// </summary>
[Verb("get-project", HelpText = "Retrieve a Bamboo project by key.")]
public class GetBambooProjectOptions
{
    [Option('k', "key", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;
}

/// <summary>
/// Options for the get-plans command.
/// </summary>
[Verb("get-plans", HelpText = "List all Bamboo plans or plans for a specific project.")]
public class GetBambooPlansOptions
{
    [Option('p', "project", Required = false, HelpText = "Filter plans by project key (optional).")]
    public string? ProjectKey { get; set; }
}

/// <summary>
/// Options for the get-plan command.
/// </summary>
[Verb("get-plan", HelpText = "Retrieve a Bamboo plan by key (includes configuration).")]
public class GetBambooPlanOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;
}

/// <summary>
/// Options for the get-branches command.
/// </summary>
[Verb("get-branches", HelpText = "List branches for a Bamboo plan.")]
public class GetBambooBranchesOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;
}

/// <summary>
/// Options for the get-builds command.
/// </summary>
[Verb("get-builds", HelpText = "List build results for a Bamboo plan.")]
public class GetBambooBuildsOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;

    [Option('n', "max-results", Default = 25, HelpText = "Maximum number of build results to return (default: 25).")]
    public int MaxResults { get; set; } = 25;
}

/// <summary>
/// Options for the get-build command.
/// </summary>
[Verb("get-build", HelpText = "Retrieve a specific build result.")]
public class GetBambooBuildOptions
{
    [Option('k', "key", Required = true, HelpText = "The build result key (e.g., PROJ-PLAN-123).")]
    public string BuildKey { get; set; } = string.Empty;
}

/// <summary>
/// Options for the get-latest-build command.
/// </summary>
[Verb("get-latest-build", HelpText = "Retrieve the latest build result for a plan.")]
public class GetBambooLatestBuildOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;
}

/// <summary>
/// Options for the queue-build command.
/// </summary>
[Verb("queue-build", HelpText = "Queue a new build for a plan (trigger a build).")]
public class QueueBambooBuildOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;

    [Option('b', "branch", Required = false, HelpText = "Branch name to build (optional, builds default branch if not specified).")]
    public string? Branch { get; set; }
}
