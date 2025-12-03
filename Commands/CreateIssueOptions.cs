using CommandLine;

namespace AtlassianCli.Commands;

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
