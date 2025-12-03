using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the queue-build command.
/// </summary>
[Verb("queue-build", HelpText = "Queue a new build for a plan (trigger a build).")]
public class QueueBambooBuildOptions
{
    [Option('k', "key", Required = true, HelpText = "The plan key (e.g., PROJ-PLAN).")]
    public string PlanKey { get; set; } = string.Empty;

    [Option('b', "branch", Required = false, HelpText = "Branch name to build (optional, builds default branch if not specified).")]
    public string? Branch { get; set; }
}
