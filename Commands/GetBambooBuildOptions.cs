using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-build command.
/// </summary>
[Verb("get-build", HelpText = "Retrieve a specific build result.")]
public class GetBambooBuildOptions
{
    [Option('k', "key", Required = true, HelpText = "The build result key (e.g., PROJ-PLAN-123).")]
    public string BuildKey { get; set; } = string.Empty;
}
