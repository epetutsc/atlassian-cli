using CommandLine;

namespace AtlassianCli.Commands;

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
