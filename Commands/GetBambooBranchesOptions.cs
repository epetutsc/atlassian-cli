using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-branches command.
/// </summary>
[Verb("get-branches", HelpText = "List branches for a Bamboo plan.")]
public class GetBambooBranchesOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;
}
