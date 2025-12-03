using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-plan command.
/// </summary>
[Verb("get-plan", HelpText = "Retrieve a Bamboo plan by key (includes configuration).")]
public class GetBambooPlanOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;
}
