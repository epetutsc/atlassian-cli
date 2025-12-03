using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the update-page command.
/// </summary>
[Verb("update-page", HelpText = "Update an existing page in Confluence.")]
public class UpdatePageOptions
{
    [Option('i', "id", SetName = "byId", HelpText = "The ID of the page to update.")]
    public string? PageId { get; set; }

    [Option('s', "space", SetName = "byTitle", HelpText = "The space key (required when using --title).")]
    public string? SpaceKey { get; set; }

    [Option('t', "title", SetName = "byTitle", HelpText = "The title of the page to update (requires --space).")]
    public string? Title { get; set; }

    [Option('b', "body", HelpText = "The new body content in Confluence storage format (XHTML). Either --body or --file is required.")]
    public string? Body { get; set; }

    [Option("file", HelpText = "Path to a UTF-8 encoded file containing the body content. Either --body or --file is required.")]
    public string? FilePath { get; set; }

    [Option('a', "append", Default = false, HelpText = "Append content to existing page instead of replacing.")]
    public bool Append { get; set; }
}
