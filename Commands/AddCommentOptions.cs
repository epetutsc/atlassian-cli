using CommandLine;

namespace AtlassianCli.Commands;

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
