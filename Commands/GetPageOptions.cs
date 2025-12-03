using CommandLine;

namespace AtlassianCli.Commands;

/// <summary>
/// Options for the get-page command.
/// </summary>
[Verb("get-page", HelpText = "Retrieve a page from Confluence by ID or by title and space.")]
public class GetPageOptions
{
    [Option('i', "id", SetName = "byId", HelpText = "The ID of the page to retrieve.")]
    public string? PageId { get; set; }

    [Option('s', "space", SetName = "byTitle", HelpText = "The space key (required when using --title).")]
    public string? SpaceKey { get; set; }

    [Option('t', "title", SetName = "byTitle", HelpText = "The title of the page to retrieve (requires --space).")]
    public string? Title { get; set; }

    [Option('f', "format", Default = "storage", HelpText = "Output format: 'storage' (raw XHTML) or 'view' (rendered HTML).")]
    public string Format { get; set; } = "storage";
}
