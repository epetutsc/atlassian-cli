using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Command-line options for getting a pull request.
/// </summary>
[Verb("get-pr", HelpText = "Get pull request information")]
public class GetPullRequestOptions
{
    [Option('p', "project", Required = true, HelpText = "Project key (e.g., PROJ)")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "Repository slug (e.g., my-repo)")]
    public string RepositorySlug { get; set; } = string.Empty;

    [Option('i', "id", Required = true, HelpText = "Pull request ID")]
    public int PullRequestId { get; set; }
}

/// <summary>
/// Command-line options for getting a pull request diff.
/// </summary>
[Verb("get-pr-diff", HelpText = "Get pull request diff")]
public class GetPullRequestDiffOptions
{
    [Option('p', "project", Required = true, HelpText = "Project key (e.g., PROJ)")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "Repository slug (e.g., my-repo)")]
    public string RepositorySlug { get; set; } = string.Empty;

    [Option('i', "id", Required = true, HelpText = "Pull request ID")]
    public int PullRequestId { get; set; }
}

/// <summary>
/// Command-line options for getting pull request commits.
/// </summary>
[Verb("get-pr-commits", HelpText = "Get pull request commits")]
public class GetPullRequestCommitsOptions
{
    [Option('p', "project", Required = true, HelpText = "Project key (e.g., PROJ)")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "Repository slug (e.g., my-repo)")]
    public string RepositorySlug { get; set; } = string.Empty;

    [Option('i', "id", Required = true, HelpText = "Pull request ID")]
    public int PullRequestId { get; set; }
}

/// <summary>
/// Command-line options for getting pull request comments.
/// </summary>
[Verb("get-pr-comments", HelpText = "Get pull request comments")]
public class GetPullRequestCommentsOptions
{
    [Option('p', "project", Required = true, HelpText = "Project key (e.g., PROJ)")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "Repository slug (e.g., my-repo)")]
    public string RepositorySlug { get; set; } = string.Empty;

    [Option('i', "id", Required = true, HelpText = "Pull request ID")]
    public int PullRequestId { get; set; }
}

/// <summary>
/// Command-line options for adding a comment to a pull request.
/// </summary>
[Verb("add-pr-comment", HelpText = "Add a comment to a pull request")]
public class AddPullRequestCommentOptions
{
    [Option('p', "project", Required = true, HelpText = "Project key (e.g., PROJ)")]
    public string ProjectKey { get; set; } = string.Empty;

    [Option('r', "repo", Required = true, HelpText = "Repository slug (e.g., my-repo)")]
    public string RepositorySlug { get; set; } = string.Empty;

    [Option('i', "id", Required = true, HelpText = "Pull request ID")]
    public int PullRequestId { get; set; }

    [Option('t', "text", Required = false, HelpText = "Comment text (either --text or --file is required)")]
    public string? Text { get; set; }

    [Option("file", Required = false, HelpText = "Path to file containing comment text")]
    public string? FilePath { get; set; }
}
