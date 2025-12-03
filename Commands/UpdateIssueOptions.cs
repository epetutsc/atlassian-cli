using CommandLine;

namespace AtlassianCli.Commands;

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
