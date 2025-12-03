using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-builds command.
/// </summary>
[Verb("get-builds", HelpText = "List build results for a Bamboo plan.")]
public class GetBambooBuildsOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;

    [Option('n', "max-results", Default = 25, HelpText = "Maximum number of build results to return (default: 25).")]
    public int MaxResults { get; set; } = 25;
}
