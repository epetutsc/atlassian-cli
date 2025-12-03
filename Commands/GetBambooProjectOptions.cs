using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-project command.
/// </summary>
[Verb("get-project", HelpText = "Retrieve a Bamboo project by key.")]
public class GetBambooProjectOptions
{
    [Option('k', "key", Required = true, HelpText = "The project key (e.g., PROJ).")]
    public string ProjectKey { get; set; } = string.Empty;
}
