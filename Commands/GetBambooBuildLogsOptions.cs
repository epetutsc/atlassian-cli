using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-build-logs command.
/// </summary>
[Verb("get-build-logs", HelpText = "Retrieve build logs from a Bamboo build result.")]
public class GetBambooBuildLogsOptions
{
    [Option('k', "key", Required = true, HelpText = "The build result key (e.g., PROJ-PLAN-123).")]
    public string BuildKey { get; set; } = string.Empty;

    [Option('f', "filter", Required = false, HelpText = "Filter log lines using regex patterns (case-insensitive). Can be specified multiple times. Lines matching ANY pattern are shown. Examples: -f error -f exception, -f 'error|exception', -f 'failed.*test'")]
    public IEnumerable<string> Filters { get; set; } = Enumerable.Empty<string>();

    [Option('j', "job", Required = false, HelpText = "Get logs for a specific job within the build (e.g., PROJ-PLAN-JOB1).")]
    public string? JobKey { get; set; }
}
