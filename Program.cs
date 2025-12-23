using CommandLine;
using CommandLine.Text;
using AtlassianCli.Commands;

namespace AtlassianCli;

/// <summary>
/// Atlassian CLI - A command-line interface for interacting with Confluence, Jira, Bamboo, and BitBucket REST APIs.
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
///   For Bamboo:
///     BAMBOO_BASE_URL         - Base URL of your Bamboo instance (e.g., https://bamboo.example.com)
///     BAMBOO_USERNAME         - Username for authentication  
///     BAMBOO_API_TOKEN        - API token (preferred) OR
///     BAMBOO_PASSWORD         - Password (if not using API token)
///
///   For BitBucket:
///     BITBUCKET_BASE_URL      - Base URL of your BitBucket instance (e.g., https://bitbucket.example.com)
///     BITBUCKET_USERNAME      - Username for authentication  
///     BITBUCKET_API_TOKEN     - API token (preferred) OR
///     BITBUCKET_PASSWORD      - Password (if not using API token)
/// 
/// Confluence Usage Examples:
///   atlassiancli confluence create-page --space KEY --title "My Title" --body "&lt;p&gt;Hello&lt;/p&gt;"
///   atlassiancli confluence get-page --id 12345
///   atlassiancli confluence get-page --space KEY --title "My Title"
///   atlassiancli confluence update-page --id 12345 --body "&lt;p&gt;Updated&lt;/p&gt;"
///   atlassiancli confluence update-page --id 12345 --body "&lt;p&gt;Appended&lt;/p&gt;" --append
///
/// Jira Usage Examples:
///   atlassiancli jira get-issue --key PROJ-123
///   atlassiancli jira create-issue --project PROJ --summary "New task" --type Task
///   atlassiancli jira add-comment --key PROJ-123 --body "My comment"
///   atlassiancli jira change-status --key PROJ-123 --status "In Progress"
///   atlassiancli jira assign-user --key PROJ-123 --user john.doe
///   atlassiancli jira update-issue --key PROJ-123 --description "Updated description"
///
/// Bamboo Usage Examples:
///   atlassiancli bamboo get-projects
///   atlassiancli bamboo get-plans --project PROJ
///   atlassiancli bamboo get-plan --key PROJ-PLAN
///   atlassiancli bamboo get-builds --key PROJ-PLAN
///   atlassiancli bamboo get-build --key PROJ-PLAN-123
///   atlassiancli bamboo queue-build --key PROJ-PLAN
///
/// BitBucket Usage Examples:
///   atlassiancli bitbucket get-pr --project PROJ --repo my-repo --id 123
///   atlassiancli bitbucket get-pr-diff --project PROJ --repo my-repo --id 123
///   atlassiancli bitbucket get-pr-commits --project PROJ --repo my-repo --id 123
///   atlassiancli bitbucket add-pr-comment --project PROJ --repo my-repo --id 123 --text "Review comment"
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Show help header when invoked with no args
        if (args.Length == 0)
        {
            ShowWelcome();
            Console.WriteLine("\nUse --help for available commands and options.");
            return 0;
        }

        // Handle top-level --help directly
        if (args.Length >= 1 && (args[0] == "--help" || args[0] == "-h" || args[0] == "help"))
        {
            ShowWelcome();
            Console.WriteLine();
            ShowTopLevelHelp();
            return 0;
        }

        var parser = new Parser(config =>
        {
            config.HelpWriter = null; // We'll handle help output ourselves for more control
            config.CaseInsensitiveEnumValues = true;
        });

        // Handle top-level sub-commands manually
        var subCommand = args[0]?.ToLowerInvariant() ?? string.Empty;
        var subArgs = args.Skip(1).ToArray();

        return subCommand switch
        {
            "confluence" => await HandleConfluenceAsync(subArgs, parser),
            "jira" => await HandleJiraAsync(subArgs, parser),
            "bamboo" => await HandleBambooAsync(subArgs, parser),
            "bitbucket" => await HandleBitBucketAsync(subArgs, parser),
            _ => HandleUnknownSubCommand(args[0])
        };
    }

    /// <summary>
    /// Handles unknown sub-commands.
    /// </summary>
    private static int HandleUnknownSubCommand(string command)
    {
        Console.Error.WriteLine($"Error: Unknown sub-command '{command}'.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Available sub-commands: confluence, jira, bamboo, bitbucket");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Use 'atlassiancli --help' for more information.");
        return 1;
    }

    /// <summary>
    /// Handles Confluence sub-commands.
    /// </summary>
    private static async Task<int> HandleConfluenceAsync(string[] args, Parser parser)
    {
        if (args.Length == 0)
        {
            ShowConfluenceHelp();
            return 0;
        }

        var parserResult = parser.ParseArguments<CreatePageOptions, GetPageOptions, UpdatePageOptions>(args);

        return await parserResult.MapResult(
            async (CreatePageOptions opts) => await ConfluenceCommandHandlers.HandleCreatePageAsync(opts),
            async (GetPageOptions opts) => await ConfluenceCommandHandlers.HandleGetPageAsync(opts),
            async (UpdatePageOptions opts) => await ConfluenceCommandHandlers.HandleUpdatePageAsync(opts),
            async errs => await HandleConfluenceParseErrorsAsync(parserResult, errs)
        );
    }

    /// <summary>
    /// Handles Jira sub-commands.
    /// </summary>
    private static async Task<int> HandleJiraAsync(string[] args, Parser parser)
    {
        if (args.Length == 0)
        {
            ShowJiraHelp();
            return 0;
        }

        var parserResult = parser.ParseArguments<
            GetIssueOptions, CreateIssueOptions, AddCommentOptions,
            ChangeStatusOptions, AssignUserOptions, UpdateIssueOptions>(args);

        return await parserResult.MapResult(
            async (GetIssueOptions opts) => await JiraCommandHandlers.HandleGetIssueAsync(opts),
            async (CreateIssueOptions opts) => await JiraCommandHandlers.HandleCreateIssueAsync(opts),
            async (AddCommentOptions opts) => await JiraCommandHandlers.HandleAddCommentAsync(opts),
            async (ChangeStatusOptions opts) => await JiraCommandHandlers.HandleChangeStatusAsync(opts),
            async (AssignUserOptions opts) => await JiraCommandHandlers.HandleAssignUserAsync(opts),
            async (UpdateIssueOptions opts) => await JiraCommandHandlers.HandleUpdateIssueAsync(opts),
            async errs => await HandleJiraParseErrorsAsync(parserResult, errs)
        );
    }

    /// <summary>
    /// Handles Bamboo sub-commands.
    /// </summary>
    private static async Task<int> HandleBambooAsync(string[] args, Parser parser)
    {
        if (args.Length == 0)
        {
            ShowBambooHelp();
            return 0;
        }

        var parserResult = parser.ParseArguments<
            GetBambooProjectsOptions, GetBambooProjectOptions, GetBambooPlansOptions,
            GetBambooPlanOptions, GetBambooBranchesOptions, GetBambooBuildsOptions,
            GetBambooBuildOptions, GetBambooLatestBuildOptions, QueueBambooBuildOptions,
            GetBambooBuildLogsOptions>(args);

        return await parserResult.MapResult(
            async (GetBambooProjectsOptions opts) => await BambooCommandHandlers.HandleGetBambooProjectsAsync(opts),
            async (GetBambooProjectOptions opts) => await BambooCommandHandlers.HandleGetBambooProjectAsync(opts),
            async (GetBambooPlansOptions opts) => await BambooCommandHandlers.HandleGetBambooPlansAsync(opts),
            async (GetBambooPlanOptions opts) => await BambooCommandHandlers.HandleGetBambooPlanAsync(opts),
            async (GetBambooBranchesOptions opts) => await BambooCommandHandlers.HandleGetBambooBranchesAsync(opts),
            async (GetBambooBuildsOptions opts) => await BambooCommandHandlers.HandleGetBambooBuildsAsync(opts),
            async (GetBambooBuildOptions opts) => await BambooCommandHandlers.HandleGetBambooBuildAsync(opts),
            async (GetBambooLatestBuildOptions opts) => await BambooCommandHandlers.HandleGetBambooLatestBuildAsync(opts),
            async (QueueBambooBuildOptions opts) => await BambooCommandHandlers.HandleQueueBambooBuildAsync(opts),
            async (GetBambooBuildLogsOptions opts) => await BambooCommandHandlers.HandleGetBambooBuildLogsAsync(opts),
            async errs => await HandleBambooParseErrorsAsync(parserResult, errs)
        );
    }

    /// <summary>
    /// Handles BitBucket sub-commands.
    /// </summary>
    private static async Task<int> HandleBitBucketAsync(string[] args, Parser parser)
    {
        if (args.Length == 0)
        {
            ShowBitBucketHelp();
            return 0;
        }

        var parserResult = parser.ParseArguments<
            GetPullRequestOptions, GetPullRequestDiffOptions, GetPullRequestCommitsOptions,
            GetPullRequestCommentsOptions, AddPullRequestCommentOptions>(args);

        return await parserResult.MapResult(
            async (GetPullRequestOptions opts) => await BitBucketCommandHandlers.HandleGetPullRequestAsync(opts),
            async (GetPullRequestDiffOptions opts) => await BitBucketCommandHandlers.HandleGetPullRequestDiffAsync(opts),
            async (GetPullRequestCommitsOptions opts) => await BitBucketCommandHandlers.HandleGetPullRequestCommitsAsync(opts),
            async (GetPullRequestCommentsOptions opts) => await BitBucketCommandHandlers.HandleGetPullRequestCommentsAsync(opts),
            async (AddPullRequestCommentOptions opts) => await BitBucketCommandHandlers.HandleAddPullRequestCommentAsync(opts),
            async errs => await HandleBitBucketParseErrorsAsync(parserResult, errs)
        );
    }

    /// <summary>
    /// Displays the welcome message with CLI information.
    /// </summary>
    private static void ShowWelcome()
    {
        Console.WriteLine("Atlassian CLI - Command-line interface for Confluence, Jira, Bamboo, and BitBucket REST APIs");
        Console.WriteLine();
        Console.WriteLine("Usage: atlassiancli <confluence|jira|bamboo|bitbucket> <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Sub-commands:");
        Console.WriteLine("  confluence    Confluence commands for managing pages");
        Console.WriteLine("  jira          Jira commands for managing issues");
        Console.WriteLine("  bamboo        Bamboo commands for viewing builds and triggering plans");
        Console.WriteLine("  bitbucket     BitBucket commands for reviewing pull requests");
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
        Console.WriteLine();
        Console.WriteLine("Bamboo Environment Variables:");
        Console.WriteLine("  BAMBOO_BASE_URL         Base URL of your Bamboo instance");
        Console.WriteLine("                          Example: https://bamboo.example.com");
        Console.WriteLine();
        Console.WriteLine("  BAMBOO_USERNAME         Username for authentication");
        Console.WriteLine();
        Console.WriteLine("  BAMBOO_API_TOKEN        API token (preferred for security)");
        Console.WriteLine("                          OR");
        Console.WriteLine("  BAMBOO_PASSWORD         Password (if not using API token)");
        Console.WriteLine();
        Console.WriteLine("BitBucket Environment Variables:");
        Console.WriteLine("  BITBUCKET_BASE_URL      Base URL of your BitBucket instance");
        Console.WriteLine("                          Example: https://bitbucket.example.com");
        Console.WriteLine();
        Console.WriteLine("  BITBUCKET_USERNAME      Username for authentication");
        Console.WriteLine();
        Console.WriteLine("  BITBUCKET_API_TOKEN     API token (preferred for security)");
        Console.WriteLine("                          OR");
        Console.WriteLine("  BITBUCKET_PASSWORD      Password (if not using API token)");
    }

    /// <summary>
    /// Displays top-level help text with examples.
    /// </summary>
    private static void ShowTopLevelHelp()
    {
        Console.WriteLine("Confluence Examples:");
        Console.WriteLine("  atlassiancli confluence create-page --space MYSPACE --title \"New Page\" --body \"<p>Content</p>\"");
        Console.WriteLine("  atlassiancli confluence get-page --id 12345");
        Console.WriteLine("  atlassiancli confluence update-page --id 12345 --body \"<p>Updated</p>\"");
        Console.WriteLine();
        Console.WriteLine("Jira Examples:");
        Console.WriteLine("  atlassiancli jira get-issue --key PROJ-123");
        Console.WriteLine("  atlassiancli jira create-issue --project PROJ --summary \"Task\" --type Task");
        Console.WriteLine("  atlassiancli jira add-comment --key PROJ-123 --body \"My comment\"");
        Console.WriteLine();
        Console.WriteLine("Bamboo Examples:");
        Console.WriteLine("  atlassiancli bamboo get-projects");
        Console.WriteLine("  atlassiancli bamboo get-plans --project PROJ");
        Console.WriteLine("  atlassiancli bamboo get-plan --key PROJ-PLAN");
        Console.WriteLine("  atlassiancli bamboo get-builds --key PROJ-PLAN");
        Console.WriteLine("  atlassiancli bamboo queue-build --key PROJ-PLAN");
        Console.WriteLine();
        Console.WriteLine("BitBucket Examples:");
        Console.WriteLine("  atlassiancli bitbucket get-pr --project PROJ --repo my-repo --id 123");
        Console.WriteLine("  atlassiancli bitbucket get-pr-diff --project PROJ --repo my-repo --id 123");
        Console.WriteLine("  atlassiancli bitbucket get-pr-commits --project PROJ --repo my-repo --id 123");
        Console.WriteLine("  atlassiancli bitbucket add-pr-comment --project PROJ --repo my-repo --id 123 --text \"Review comment\"");
        Console.WriteLine();
        Console.WriteLine("Use 'atlassiancli <confluence|jira|bamboo|bitbucket>' for more information about a sub-command.");
    }

    /// <summary>
    /// Displays help for Confluence sub-commands.
    /// </summary>
    private static void ShowConfluenceHelp()
    {
        Console.WriteLine("Atlassian CLI - Confluence Commands");
        Console.WriteLine();
        Console.WriteLine("Usage: atlassiancli confluence <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  create-page   Create a new page in a specified space");
        Console.WriteLine("  get-page      Retrieve a page by ID or by title and space");
        Console.WriteLine("  update-page   Update an existing page's content");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  atlassiancli confluence create-page --space MYSPACE --title \"New Page\" --body \"<p>Content</p>\"");
        Console.WriteLine("  atlassiancli confluence get-page --id 12345");
        Console.WriteLine("  atlassiancli confluence get-page --space MYSPACE --title \"Page Title\"");
        Console.WriteLine("  atlassiancli confluence update-page --id 12345 --body \"<p>Updated</p>\"");
        Console.WriteLine("  atlassiancli confluence update-page --id 12345 --body \"<p>More</p>\" --append");
        Console.WriteLine();
        Console.WriteLine("Use 'atlassiancli confluence <command> --help' for more information about a command.");
    }

    /// <summary>
    /// Displays help for Jira sub-commands.
    /// </summary>
    private static void ShowJiraHelp()
    {
        Console.WriteLine("Atlassian CLI - Jira Commands");
        Console.WriteLine();
        Console.WriteLine("Usage: atlassiancli jira <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  get-issue       Retrieve an issue by key");
        Console.WriteLine("  create-issue    Create a new issue in a project");
        Console.WriteLine("  add-comment     Add a comment to an issue");
        Console.WriteLine("  change-status   Change the status of an issue");
        Console.WriteLine("  assign-user     Assign a user to an issue");
        Console.WriteLine("  update-issue    Update an issue's description");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  atlassiancli jira get-issue --key PROJ-123");
        Console.WriteLine("  atlassiancli jira create-issue --project PROJ --summary \"Task\" --type Task");
        Console.WriteLine("  atlassiancli jira add-comment --key PROJ-123 --body \"My comment\"");
        Console.WriteLine("  atlassiancli jira change-status --key PROJ-123 --status \"In Progress\"");
        Console.WriteLine("  atlassiancli jira assign-user --key PROJ-123 --user john.doe");
        Console.WriteLine("  atlassiancli jira update-issue --key PROJ-123 --description \"Updated\"");
        Console.WriteLine();
        Console.WriteLine("Use 'atlassiancli jira <command> --help' for more information about a command.");
    }

    /// <summary>
    /// Displays help for Bamboo sub-commands.
    /// </summary>
    private static void ShowBambooHelp()
    {
        Console.WriteLine("Atlassian CLI - Bamboo Commands");
        Console.WriteLine();
        Console.WriteLine("Usage: atlassiancli bamboo <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  get-projects       List all Bamboo projects");
        Console.WriteLine("  get-project        Retrieve a project by key");
        Console.WriteLine("  get-plans          List all plans (optionally filtered by project)");
        Console.WriteLine("  get-plan           Retrieve a plan by key (includes configuration)");
        Console.WriteLine("  get-branches       List branches for a plan");
        Console.WriteLine("  get-builds         List build results for a plan");
        Console.WriteLine("  get-build          Retrieve a specific build result");
        Console.WriteLine("  get-latest-build   Retrieve the latest build result for a plan");
        Console.WriteLine("  get-build-logs     Retrieve build logs (with optional filter)");
        Console.WriteLine("  queue-build        Queue a new build for a plan (trigger build)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  atlassiancli bamboo get-projects");
        Console.WriteLine("  atlassiancli bamboo get-project --key PROJ");
        Console.WriteLine("  atlassiancli bamboo get-plans --project PROJ");
        Console.WriteLine("  atlassiancli bamboo get-plan --key PROJ-PLAN");
        Console.WriteLine("  atlassiancli bamboo get-branches --key PROJ-PLAN");
        Console.WriteLine("  atlassiancli bamboo get-builds --key PROJ-PLAN --max-results 10");
        Console.WriteLine("  atlassiancli bamboo get-build --key PROJ-PLAN-123");
        Console.WriteLine("  atlassiancli bamboo get-latest-build --key PROJ-PLAN");
        Console.WriteLine("  atlassiancli bamboo get-build-logs --key PROJ-PLAN-123");
        Console.WriteLine("  atlassiancli bamboo get-build-logs --key PROJ-PLAN-123 -f error -f exception");
        Console.WriteLine("  atlassiancli bamboo get-build-logs --key PROJ-PLAN-123 -f 'error|exception'");
        Console.WriteLine("  atlassiancli bamboo get-build-logs --key PROJ-PLAN-123 -f 'failed.*test'");
        Console.WriteLine("  atlassiancli bamboo queue-build --key PROJ-PLAN");
        Console.WriteLine("  atlassiancli bamboo queue-build --key PROJ-PLAN --branch feature/my-branch");
        Console.WriteLine();
        Console.WriteLine("Use 'atlassiancli bamboo <command> --help' for more information about a command.");
    }

    /// <summary>
    /// Displays help for BitBucket sub-commands.
    /// </summary>
    private static void ShowBitBucketHelp()
    {
        Console.WriteLine("Atlassian CLI - BitBucket Commands");
        Console.WriteLine();
        Console.WriteLine("Usage: atlassiancli bitbucket <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  get-pr            Get pull request information");
        Console.WriteLine("  get-pr-diff       Get pull request diff");
        Console.WriteLine("  get-pr-commits    Get pull request commits");
        Console.WriteLine("  get-pr-comments   Get pull request comments");
        Console.WriteLine("  add-pr-comment    Add a comment to a pull request");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  atlassiancli bitbucket get-pr --project PROJ --repo my-repo --id 123");
        Console.WriteLine("  atlassiancli bitbucket get-pr-diff --project PROJ --repo my-repo --id 123");
        Console.WriteLine("  atlassiancli bitbucket get-pr-commits --project PROJ --repo my-repo --id 123");
        Console.WriteLine("  atlassiancli bitbucket get-pr-comments --project PROJ --repo my-repo --id 123");
        Console.WriteLine("  atlassiancli bitbucket add-pr-comment --project PROJ --repo my-repo --id 123 --text \"Review comment\"");
        Console.WriteLine("  atlassiancli bitbucket add-pr-comment --project PROJ --repo my-repo --id 123 --file comment.txt");
        Console.WriteLine();
        Console.WriteLine("Use 'atlassiancli bitbucket <command> --help' for more information about a command.");
    }

    /// <summary>
    /// Handles parsing errors for Confluence sub-commands.
    /// </summary>
    private static Task<int> HandleConfluenceParseErrorsAsync<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "Atlassian CLI v1.0.0 - Confluence";
            h.Copyright = "Commands for managing Confluence pages";
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Usage: atlassiancli confluence <command> [options]");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Commands:");
            h.AddPreOptionsLine("  create-page   Create a new page in a specified space");
            h.AddPreOptionsLine("  get-page      Retrieve a page by ID or by title and space");
            h.AddPreOptionsLine("  update-page   Update an existing page's content");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Examples:");
            h.AddPreOptionsLine("  atlassiancli confluence create-page --space MYSPACE --title \"New Page\" --body \"<p>Content</p>\"");
            h.AddPreOptionsLine("  atlassiancli confluence get-page --id 12345");
            h.AddPreOptionsLine("  atlassiancli confluence get-page --space MYSPACE --title \"Page Title\"");
            h.AddPreOptionsLine("  atlassiancli confluence update-page --id 12345 --body \"<p>Updated</p>\"");
            h.AddPreOptionsLine("  atlassiancli confluence update-page --id 12345 --body \"<p>More</p>\" --append");
            return h;
        }, e => e);

        Console.WriteLine(helpText);

        if (errors.IsHelp() || errors.IsVersion())
        {
            return Task.FromResult(0);
        }

        return Task.FromResult(1);
    }

    /// <summary>
    /// Handles parsing errors for Jira sub-commands.
    /// </summary>
    private static Task<int> HandleJiraParseErrorsAsync<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "Atlassian CLI v1.0.0 - Jira";
            h.Copyright = "Commands for managing Jira issues";
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Usage: atlassiancli jira <command> [options]");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Commands:");
            h.AddPreOptionsLine("  get-issue       Retrieve an issue by key");
            h.AddPreOptionsLine("  create-issue    Create a new issue in a project");
            h.AddPreOptionsLine("  add-comment     Add a comment to an issue");
            h.AddPreOptionsLine("  change-status   Change the status of an issue");
            h.AddPreOptionsLine("  assign-user     Assign a user to an issue");
            h.AddPreOptionsLine("  update-issue    Update an issue's description");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Examples:");
            h.AddPreOptionsLine("  atlassiancli jira get-issue --key PROJ-123");
            h.AddPreOptionsLine("  atlassiancli jira create-issue --project PROJ --summary \"Task\" --type Task");
            h.AddPreOptionsLine("  atlassiancli jira add-comment --key PROJ-123 --body \"My comment\"");
            h.AddPreOptionsLine("  atlassiancli jira change-status --key PROJ-123 --status \"In Progress\"");
            h.AddPreOptionsLine("  atlassiancli jira assign-user --key PROJ-123 --user john.doe");
            h.AddPreOptionsLine("  atlassiancli jira update-issue --key PROJ-123 --description \"Updated\"");
            return h;
        }, e => e);

        Console.WriteLine(helpText);

        if (errors.IsHelp() || errors.IsVersion())
        {
            return Task.FromResult(0);
        }

        return Task.FromResult(1);
    }

    /// <summary>
    /// Handles parsing errors for Bamboo sub-commands.
    /// </summary>
    private static Task<int> HandleBambooParseErrorsAsync<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "Atlassian CLI v1.0.0 - Bamboo";
            h.Copyright = "Commands for viewing builds and triggering plans";
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Usage: atlassiancli bamboo <command> [options]");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Commands:");
            h.AddPreOptionsLine("  get-projects       List all Bamboo projects");
            h.AddPreOptionsLine("  get-project        Retrieve a project by key");
            h.AddPreOptionsLine("  get-plans          List all plans (optionally filtered by project)");
            h.AddPreOptionsLine("  get-plan           Retrieve a plan by key (includes configuration)");
            h.AddPreOptionsLine("  get-branches       List branches for a plan");
            h.AddPreOptionsLine("  get-builds         List build results for a plan");
            h.AddPreOptionsLine("  get-build          Retrieve a specific build result");
            h.AddPreOptionsLine("  get-latest-build   Retrieve the latest build for a plan");
            h.AddPreOptionsLine("  get-build-logs     Retrieve build logs (with optional regex filters)");
            h.AddPreOptionsLine("  queue-build        Queue a new build for a plan");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Examples:");
            h.AddPreOptionsLine("  atlassiancli bamboo get-projects");
            h.AddPreOptionsLine("  atlassiancli bamboo get-plan --key PROJ-PLAN");
            h.AddPreOptionsLine("  atlassiancli bamboo get-builds --key PROJ-PLAN");
            h.AddPreOptionsLine("  atlassiancli bamboo get-build-logs --key PROJ-PLAN-123 -f error -f exception");
            h.AddPreOptionsLine("  atlassiancli bamboo get-build-logs --key PROJ-PLAN-123 -f 'error|exception'");
            h.AddPreOptionsLine("  atlassiancli bamboo queue-build --key PROJ-PLAN");
            return h;
        }, e => e);

        Console.WriteLine(helpText);

        if (errors.IsHelp() || errors.IsVersion())
        {
            return Task.FromResult(0);
        }

        return Task.FromResult(1);
    }

    /// <summary>
    /// Handles parsing errors for BitBucket sub-commands.
    /// </summary>
    private static Task<int> HandleBitBucketParseErrorsAsync<T>(ParserResult<T> result, IEnumerable<Error> errors)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = "Atlassian CLI v1.0.0 - BitBucket";
            h.Copyright = "Commands for reviewing pull requests";
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Usage: atlassiancli bitbucket <command> [options]");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Commands:");
            h.AddPreOptionsLine("  get-pr            Get pull request information");
            h.AddPreOptionsLine("  get-pr-diff       Get pull request diff");
            h.AddPreOptionsLine("  get-pr-commits    Get pull request commits");
            h.AddPreOptionsLine("  get-pr-comments   Get pull request comments");
            h.AddPreOptionsLine("  add-pr-comment    Add a comment to a pull request");
            h.AddPreOptionsLine("");
            h.AddPreOptionsLine("Examples:");
            h.AddPreOptionsLine("  atlassiancli bitbucket get-pr --project PROJ --repo my-repo --id 123");
            h.AddPreOptionsLine("  atlassiancli bitbucket get-pr-diff --project PROJ --repo my-repo --id 123");
            h.AddPreOptionsLine("  atlassiancli bitbucket add-pr-comment --project PROJ --repo my-repo --id 123 --text \"Review comment\"");
            return h;
        }, e => e);

        Console.WriteLine(helpText);

        if (errors.IsHelp() || errors.IsVersion())
        {
            return Task.FromResult(0);
        }

        return Task.FromResult(1);
    }
}
