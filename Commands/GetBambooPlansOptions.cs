using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-plans command.
/// </summary>
[Verb("get-plans", HelpText = "List all Bamboo plans or plans for a specific project.")]
public class GetBambooPlansOptions
{
    [Option('p', "project", Required = false, HelpText = "Filter plans by project key (optional).")]
    public string? ProjectKey { get; set; }
}
