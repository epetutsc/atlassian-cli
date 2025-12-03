using CommandLine;

namespace AtlassianCli.Commands;

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
