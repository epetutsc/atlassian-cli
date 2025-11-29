using CommandLine;
using CommandLine.Text;
using AtlassianCli.Commands;

namespace AtlassianCli;

/// <summary>
/// Atlassian CLI - A command-line interface for interacting with Confluence and Jira REST APIs.
/// 
/// Environment Variables Required:
///   For Confluence:
///     CONFLUENCE_BASE_URL     - Base URL of your Confluence instance (e.g., https://confluence.example.com)
///     CONFLUENCE_USERNAME     - Username for authentication
///     CONFLUENCE_API_TOKEN    - API token (preferred) OR
///     CONFLUENCE_PASSWORD     - Password (if not using API token)
/// 
///   For Jira:
///     JIRA_BASE_URL           - Base URL of your Jira instance (e.g., https://jira.example.com)
///     JIRA_USERNAME           - Username for authentication  
///     JIRA_API_TOKEN          - API token (preferred) OR
///     JIRA_PASSWORD           - Password (if not using API token)
/// 
/// Confluence Usage Examples:
///   atlassiancli create-page --space KEY --title "My Title" --body "&lt;p&gt;Hello&lt;/p&gt;"
///   atlassiancli get-page --id 12345
///   atlassiancli get-page --space KEY --title "My Title"
///   atlassiancli update-page --id 12345 --body "&lt;p&gt;Updated&lt;/p&gt;"
///   atlassiancli update-page --id 12345 --body "&lt;p&gt;Appended&lt;/p&gt;" --append
///
/// Jira Usage Examples:
///   atlassiancli get-issue --key PROJ-123
///   atlassiancli create-issue --project PROJ --summary "New task" --type Task
///   atlassiancli add-comment --key PROJ-123 --body "My comment"
///   atlassiancli change-status --key PROJ-123 --status "In Progress"
///   atlassiancli assign-user --key PROJ-123 --user john.doe
///   atlassiancli update-issue --key PROJ-123 --description "Updated description"
/// </summary>
public static class Program
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

        var parserResult = parser.ParseArguments<
            // Confluence commands
            CreatePageOptions, GetPageOptions, UpdatePageOptions,
            // Jira commands
            GetIssueOptions, CreateIssueOptions, AddCommentOptions, 
            ChangeStatusOptions, AssignUserOptions, UpdateIssueOptions>(args);

        return await parserResult.MapResult(
            // Confluence handlers
            async (CreatePageOptions opts) => await CommandHandlers.HandleCreatePageAsync(opts),
            async (GetPageOptions opts) => await CommandHandlers.HandleGetPageAsync(opts),
            async (UpdatePageOptions opts) => await CommandHandlers.HandleUpdatePageAsync(opts),
            // Jira handlers
            async (GetIssueOptions opts) => await CommandHandlers.HandleGetIssueAsync(opts),
            async (CreateIssueOptions opts) => await CommandHandlers.HandleCreateIssueAsync(opts),
            async (AddCommentOptions opts) => await CommandHandlers.HandleAddCommentAsync(opts),
            async (ChangeStatusOptions opts) => await CommandHandlers.HandleChangeStatusAsync(opts),
            async (AssignUserOptions opts) => await CommandHandlers.HandleAssignUserAsync(opts),
            async (UpdateIssueOptions opts) => await CommandHandlers.HandleUpdateIssueAsync(opts),
            async errs => await HandleParseErrorsAsync(parserResult, errs)
        );
    }

    /// <summary>
    /// Displays the welcome message with CLI information.
    /// </summary>
    private static void ShowWelcome()
    {
        Console.WriteLine("Atlassian CLI - Command-line interface for Confluence and Jira REST APIs");
        Console.WriteLine();
        Console.WriteLine("Confluence Environment Variables:");
        Console.WriteLine("  CONFLUENCE_BASE_URL     Base URL of your Confluence instance");
        Console.WriteLine("                          Example: https://confluence.example.com");
        Console.WriteLine();
        Console.WriteLine("  CONFLUENCE_USERNAME     Username for authentication");
        Console.WriteLine();
        Console.WriteLine("  CONFLUENCE_API_TOKEN    API token (preferred for security)");
        Console.WriteLine("                          OR");
        Console.WriteLine("  CONFLUENCE_PASSWORD     Password (if not using API token)");
        Console.WriteLine();
        Console.WriteLine("Jira Environment Variables:");
        Console.WriteLine("  JIRA_BASE_URL           Base URL of your Jira instance");
        Console.WriteLine("                          Example: https://jira.example.com");
        Console.WriteLine();
        Console.WriteLine("  JIRA_USERNAME           Username for authentication");
        Console.WriteLine();
        Console.WriteLine("  JIRA_API_TOKEN          API token (preferred for security)");
        Console.WriteLine("                          OR");
        Console.WriteLine("  JIRA_PASSWORD           Password (if not using API token)");
    }

    /// <summary>
    /// Handles parsing errors and displays appropriate help information.
    /// </summary>
    private static Task<int> HandleParseErrorsAsync<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "Atlassian CLI v1.0.0";
            h.Copyright = "A command-line interface for Confluence and Jira REST APIs";
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Confluence Environment Variables:");
            h.AddPreOptionsLine("  CONFLUENCE_BASE_URL     - Base URL (e.g., https://confluence.example.com)");
            h.AddPreOptionsLine("  CONFLUENCE_USERNAME     - Username for authentication");
            h.AddPreOptionsLine("  CONFLUENCE_API_TOKEN    - API token (or use CONFLUENCE_PASSWORD)");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Jira Environment Variables:");
            h.AddPreOptionsLine("  JIRA_BASE_URL           - Base URL (e.g., https://jira.example.com)");
            h.AddPreOptionsLine("  JIRA_USERNAME           - Username for authentication");
            h.AddPreOptionsLine("  JIRA_API_TOKEN          - API token (or use JIRA_PASSWORD)");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Confluence Commands:");
            h.AddPreOptionsLine("  create-page   Create a new page in a specified space");
            h.AddPreOptionsLine("  get-page      Retrieve a page by ID or by title and space");
            h.AddPreOptionsLine("  update-page   Update an existing page's content");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Jira Commands:");
            h.AddPreOptionsLine("  get-issue       Retrieve an issue by key");
            h.AddPreOptionsLine("  create-issue    Create a new issue in a project");
            h.AddPreOptionsLine("  add-comment     Add a comment to an issue");
            h.AddPreOptionsLine("  change-status   Change the status of an issue");
            h.AddPreOptionsLine("  assign-user     Assign a user to an issue");
            h.AddPreOptionsLine("  update-issue    Update an issue's description");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Confluence Examples:");
            h.AddPreOptionsLine("  atlassiancli create-page --space MYSPACE --title \"New Page\" --body \"<p>Content</p>\"");
            h.AddPreOptionsLine("  atlassiancli get-page --id 12345");
            h.AddPreOptionsLine("  atlassiancli get-page --space MYSPACE --title \"Page Title\"");
            h.AddPreOptionsLine("  atlassiancli update-page --id 12345 --body \"<p>Updated</p>\"");
            h.AddPreOptionsLine("  atlassiancli update-page --id 12345 --body \"<p>More</p>\" --append");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Jira Examples:");
            h.AddPreOptionsLine("  atlassiancli get-issue --key PROJ-123");
            h.AddPreOptionsLine("  atlassiancli create-issue --project PROJ --summary \"Task\" --type Task");
            h.AddPreOptionsLine("  atlassiancli add-comment --key PROJ-123 --body \"My comment\"");
            h.AddPreOptionsLine("  atlassiancli change-status --key PROJ-123 --status \"In Progress\"");
            h.AddPreOptionsLine("  atlassiancli assign-user --key PROJ-123 --user john.doe");
            h.AddPreOptionsLine("  atlassiancli update-issue --key PROJ-123 --description \"Updated\"");
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
