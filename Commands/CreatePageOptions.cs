using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the create-page command.
/// </summary>
[Verb("create-page", HelpText = "Create a new page in Confluence.")]
public class CreatePageOptions
{
    [Option('s', "space", Required = true, HelpText = "The space key where the page will be created (e.g., 'MYSPACE').")]
    public string SpaceKey { get; set; } = string.Empty;

    [Option('t', "title", Required = true, HelpText = "The title of the new page.")]
    public string Title { get; set; } = string.Empty;

    [Option('b', "body", HelpText = "The body content in Confluence storage format (XHTML) or plain text. Either --body or --file is required.")]
    public string? Body { get; set; }

    [Option("file", HelpText = "Path to a UTF-8 encoded file containing the body content. Either --body or --file is required.")]
    public string? FilePath { get; set; }
}
