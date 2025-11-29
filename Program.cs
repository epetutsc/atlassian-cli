using CommandLine;
using CommandLine.Text;
using ConfluenceCli.Commands;

namespace ConfluenceCli;

/// <summary>
/// Confluence CLI - A command-line interface for interacting with Confluence REST API.
/// 
/// Environment Variables Required:
///   CONFLUENCE_BASE_URL     - Base URL of your Confluence instance (e.g., https://confluence.example.com)
///   CONFLUENCE_USERNAME     - Username for authentication
///   CONFLUENCE_API_TOKEN    - API token (preferred) OR
///   CONFLUENCE_PASSWORD     - Password (if not using API token)
/// 
/// Usage Examples:
///   confluencecli create-page --space KEY --title "My Title" --body "&lt;p&gt;Hello&lt;/p&gt;"
///   confluencecli get-page --id 12345
///   confluencecli get-page --space KEY --title "My Title"
///   confluencecli update-page --id 12345 --body "&lt;p&gt;Updated&lt;/p&gt;"
///   confluencecli update-page --id 12345 --body "&lt;p&gt;Appended&lt;/p&gt;" --append
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Show help header when invoked with no args or --help
        if (args.Length == 0)
        {
            ShowWelcome();
            Console.WriteLine("\nUse --help for available commands and options.");
            return 0;
        }

        var parser = new Parser(config =>
        {
            config.HelpWriter = null; // We'll handle help output ourselves for more control
            config.CaseInsensitiveEnumValues = true;
        });

        var parserResult = parser.ParseArguments<CreatePageOptions, GetPageOptions, UpdatePageOptions>(args);

        return await parserResult.MapResult(
            async (CreatePageOptions opts) => await CommandHandlers.HandleCreatePageAsync(opts),
            async (GetPageOptions opts) => await CommandHandlers.HandleGetPageAsync(opts),
            async (UpdatePageOptions opts) => await CommandHandlers.HandleUpdatePageAsync(opts),
            async errs => await HandleParseErrorsAsync(parserResult, errs)
        );
    }

    /// <summary>
    /// Displays the welcome message with CLI information.
    /// </summary>
    private static void ShowWelcome()
    {
        Console.WriteLine("Confluence CLI - Command-line interface for Confluence REST API");
        Console.WriteLine();
        Console.WriteLine("Environment Variables:");
        Console.WriteLine("  CONFLUENCE_BASE_URL     Base URL of your Confluence instance");
        Console.WriteLine("                          Example: https://confluence.example.com");
        Console.WriteLine();
        Console.WriteLine("  CONFLUENCE_USERNAME     Username for authentication");
        Console.WriteLine();
        Console.WriteLine("  CONFLUENCE_API_TOKEN    API token (preferred for security)");
        Console.WriteLine("                          OR");
        Console.WriteLine("  CONFLUENCE_PASSWORD     Password (if not using API token)");
    }

    /// <summary>
    /// Handles parsing errors and displays appropriate help information.
    /// </summary>
    private static Task<int> HandleParseErrorsAsync<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "Confluence CLI v1.0.0";
            h.Copyright = "A command-line interface for Confluence REST API";
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Environment Variables Required:");
            h.AddPreOptionsLine("  CONFLUENCE_BASE_URL     - Base URL (e.g., https://confluence.example.com)");
            h.AddPreOptionsLine("  CONFLUENCE_USERNAME     - Username for authentication");
            h.AddPreOptionsLine("  CONFLUENCE_API_TOKEN    - API token (or use CONFLUENCE_PASSWORD)");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Commands:");
            h.AddPreOptionsLine("  create-page   Create a new page in a specified space");
            h.AddPreOptionsLine("  get-page      Retrieve a page by ID or by title and space");
            h.AddPreOptionsLine("  update-page   Update an existing page's content");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Examples:");
            h.AddPreOptionsLine("  confluencecli create-page --space MYSPACE --title \"New Page\" --body \"<p>Content</p>\"");
            h.AddPreOptionsLine("  confluencecli get-page --id 12345");
            h.AddPreOptionsLine("  confluencecli get-page --space MYSPACE --title \"Page Title\"");
            h.AddPreOptionsLine("  confluencecli update-page --id 12345 --body \"<p>Updated</p>\"");
            h.AddPreOptionsLine("  confluencecli update-page --id 12345 --body \"<p>More</p>\" --append");
            return h;
        }, e => e);

        Console.WriteLine(helpText);

        // Return 0 for help/version requests, 1 for actual errors
        if (errors.IsHelp() || errors.IsVersion())
        {
            return Task.FromResult(0);
        }

        return Task.FromResult(1);
    }
}
