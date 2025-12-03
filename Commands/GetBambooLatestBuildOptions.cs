using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-latest-build command.
/// </summary>
[Verb("get-latest-build", HelpText = "Retrieve the latest build result for a plan.")]
public class GetBambooLatestBuildOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;
}
