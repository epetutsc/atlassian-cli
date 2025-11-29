using CommandLine;

namespace ConfluenceCli.Commands;

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

    [Option('b', "body", Required = true, HelpText = "The body content in Confluence storage format (XHTML) or plain text.")]
    public string Body { get; set; } = string.Empty;
}

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

    [Option('b', "body", Required = true, HelpText = "The new body content in Confluence storage format (XHTML).")]
    public string Body { get; set; } = string.Empty;

    [Option('a', "append", Default = false, HelpText = "Append content to existing page instead of replacing.")]
    public bool Append { get; set; }
}
