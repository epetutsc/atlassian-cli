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

    [Option('b', "body", Required = true, HelpText = "The body content in Confluence storage format (XHTML) or plain text.")]
    public string Body { get; set; } = string.Empty;
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

    [Option('b', "body", Required = true, HelpText = "The new body content in Confluence storage format (XHTML).")]
    public string Body { get; set; } = string.Empty;

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

    [Option('d', "description", Required = false, HelpText = "The issue description.")]
    public string? Description { get; set; }
}

/// <summary>
/// Options for the add-comment command.
/// </summary>
[Verb("add-comment", HelpText = "Add a comment to a Jira issue.")]
public class AddCommentOptions
{
    [Option('k', "key", Required = true, HelpText = "The issue key (e.g., PROJ-123).")]
    public string IssueKey { get; set; } = string.Empty;

    [Option('b', "body", Required = true, HelpText = "The comment body text.")]
    public string Body { get; set; } = string.Empty;
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

    [Option('d', "description", Required = true, HelpText = "The new description for the issue.")]
    public string Description { get; set; } = string.Empty;
}
