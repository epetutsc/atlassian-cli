using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-issue command.
/// </summary>
[Verb("get-issue", HelpText = "Retrieve a Jira issue by key.")]
public class GetIssueOptions
{
    [Option('k', "key", Required = true, HelpText = "The issue key (e.g., PROJ-123).")]
    public string IssueKey { get; set; } = string.Empty;
}
