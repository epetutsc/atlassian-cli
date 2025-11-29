using AtlassianCli.Client;
using AtlassianCli.Models;
using System.Text;

namespace AtlassianCli.Commands;

/// <summary>
/// Handles execution of CLI commands for Confluence and Jira operations.
/// </summary>
public static class CommandHandlers
{
    /// <summary>
    /// Maximum number of comments to display when showing issue details.
    /// </summary>
    private const int MaxDisplayedComments = 5;

    // ==================== Helper Methods ====================

    /// <summary>
    /// Resolves content from either a direct string value or a file path.
    /// Files are read as UTF-8 encoded text.
    /// </summary>
    /// <param name="directContent">Direct content string (optional).</param>
    /// <param name="filePath">Path to a file containing the content (optional).</param>
    /// <param name="contentName">Name of the content for error messages (e.g., "body", "description").</param>
    /// <returns>The resolved content string.</returns>
    /// <exception cref="ArgumentException">Thrown when neither or both options are provided.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    private static string ResolveContent(string? directContent, string? filePath, string contentName)
    {
        bool hasDirectContent = !string.IsNullOrEmpty(directContent);
        bool hasFilePath = !string.IsNullOrEmpty(filePath);

        if (hasDirectContent && hasFilePath)
        {
            throw new ArgumentException($"You cannot specify both --{contentName} and --file. Please use only one.");
        }

        if (!hasDirectContent && !hasFilePath)
        {
            throw new ArgumentException($"You must specify either --{contentName} or --file.");
        }

        if (hasFilePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified file does not exist: {filePath}");
            }

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        return directContent!;
    }

    /// <summary>
    /// Resolves optional content from either a direct string value or a file path.
    /// Files are read as UTF-8 encoded text.
    /// </summary>
    /// <param name="directContent">Direct content string (optional).</param>
    /// <param name="filePath">Path to a file containing the content (optional).</param>
    /// <param name="contentOptionName">Name of the content option for error messages (e.g., "description").</param>
    /// <param name="fileOptionName">Name of the file option for error messages (e.g., "description-file").</param>
    /// <returns>The resolved content string, or null if neither is provided.</returns>
    /// <exception cref="ArgumentException">Thrown when both options are provided.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    private static string? ResolveOptionalContent(string? directContent, string? filePath, string contentOptionName, string fileOptionName)
    {
        bool hasDirectContent = !string.IsNullOrEmpty(directContent);
        bool hasFilePath = !string.IsNullOrEmpty(filePath);

        if (hasDirectContent && hasFilePath)
        {
            throw new ArgumentException($"You cannot specify both --{contentOptionName} and --{fileOptionName}. Please use only one.");
        }

        if (!hasDirectContent && !hasFilePath)
        {
            return null;
        }

        if (hasFilePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified file does not exist: {filePath}");
            }

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        return directContent;
    }

    // ==================== Confluence Handlers ====================

    /// <summary>
    /// Handles the create-page command.
    /// </summary>
    public static async Task<int> HandleCreatePageAsync(CreatePageOptions options)
    {
        try
        {
            string body = ResolveContent(options.Body, options.FilePath, "body");

            using var client = new ConfluenceClient();

            Console.WriteLine($"Creating page '{options.Title}' in space '{options.SpaceKey}'...");
            
            var page = await client.CreatePageAsync(options.SpaceKey, options.Title, body);
            
            Console.WriteLine();
            Console.WriteLine("Page created successfully!");
            Console.WriteLine($"  ID:    {page.Id}");
            Console.WriteLine($"  Title: {page.Title}");
            Console.WriteLine($"  Space: {page.Space?.Key ?? "N/A"}");
            if (page.Links?.WebUi != null)
            {
                Console.WriteLine($"  URL:   {page.Links.WebUi}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the get-page command.
    /// </summary>
    public static async Task<int> HandleGetPageAsync(GetPageOptions options)
    {
        try
        {
            // Validate options
            if (string.IsNullOrEmpty(options.PageId) && 
                (string.IsNullOrEmpty(options.SpaceKey) || string.IsNullOrEmpty(options.Title)))
            {
                Console.Error.WriteLine("Error: You must provide either --id or both --space and --title.");
                return 1;
            }

            using var client = new ConfluenceClient();

            ConfluencePage? page;

            if (!string.IsNullOrEmpty(options.PageId))
            {
                Console.WriteLine($"Fetching page with ID '{options.PageId}'...");
                page = await client.GetPageByIdAsync(options.PageId);
            }
            else
            {
                Console.WriteLine($"Searching for page '{options.Title}' in space '{options.SpaceKey}'...");
                page = await client.GetPageByTitleAsync(options.SpaceKey!, options.Title!);
            }

            if (page == null)
            {
                Console.Error.WriteLine("Page not found.");
                return 1;
            }

            Console.WriteLine();
            DisplayPage(page, options.Format);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the update-page command.
    /// </summary>
    public static async Task<int> HandleUpdatePageAsync(UpdatePageOptions options)
    {
        try
        {
            // Validate options
            if (string.IsNullOrEmpty(options.PageId) && 
                (string.IsNullOrEmpty(options.SpaceKey) || string.IsNullOrEmpty(options.Title)))
            {
                Console.Error.WriteLine("Error: You must provide either --id or both --space and --title.");
                return 1;
            }

            string body = ResolveContent(options.Body, options.FilePath, "body");

            using var client = new ConfluenceClient();

            ConfluencePage page;

            if (!string.IsNullOrEmpty(options.PageId))
            {
                Console.WriteLine($"Updating page with ID '{options.PageId}'...");
                page = await client.UpdatePageAsync(options.PageId, body, options.Append);
            }
            else
            {
                Console.WriteLine($"Updating page '{options.Title}' in space '{options.SpaceKey}'...");
                page = await client.UpdatePageByTitleAsync(options.SpaceKey!, options.Title!, body, options.Append);
            }

            Console.WriteLine();
            Console.WriteLine("Page updated successfully!");
            Console.WriteLine($"  ID:      {page.Id}");
            Console.WriteLine($"  Title:   {page.Title}");
            Console.WriteLine($"  Version: {page.Version?.Number ?? 0}");
            if (page.Links?.WebUi != null)
            {
                Console.WriteLine($"  URL:     {page.Links.WebUi}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Displays page information to the console.
    /// </summary>
    private static void DisplayPage(ConfluencePage page, string format)
    {
        Console.WriteLine("=== Page Information ===");
        Console.WriteLine($"ID:      {page.Id}");
        Console.WriteLine($"Title:   {page.Title}");
        Console.WriteLine($"Space:   {page.Space?.Key ?? "N/A"}");
        Console.WriteLine($"Status:  {page.Status}");
        Console.WriteLine($"Version: {page.Version?.Number ?? 0}");
        
        if (page.Links?.WebUi != null)
        {
            Console.WriteLine($"URL:     {page.Links.WebUi}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Body Content ===");

        string? content = format.ToLowerInvariant() switch
        {
            "view" => page.Body?.View?.Value,
            "storage" => page.Body?.Storage?.Value,
            _ => page.Body?.Storage?.Value
        };

        if (!string.IsNullOrEmpty(content))
        {
            Console.WriteLine(content);
        }
        else
        {
            Console.WriteLine("(No content available)");
        }
    }

    // ==================== Jira Handlers ====================

    /// <summary>
    /// Handles the get-issue command.
    /// </summary>
    public static async Task<int> HandleGetIssueAsync(GetIssueOptions options)
    {
        try
        {
            using var client = new JiraClient();

            Console.WriteLine($"Fetching issue '{options.IssueKey}'...");
            
            var issue = await client.GetIssueAsync(options.IssueKey);
            
            Console.WriteLine();
            DisplayIssue(issue);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the create-issue command.
    /// </summary>
    public static async Task<int> HandleCreateIssueAsync(CreateIssueOptions options)
    {
        try
        {
            string? description = ResolveOptionalContent(options.Description, options.DescriptionFilePath, "description", "description-file");

            using var client = new JiraClient();

            Console.WriteLine($"Creating {options.IssueType} in project '{options.ProjectKey}'...");
            
            var response = await client.CreateIssueAsync(
                options.ProjectKey, 
                options.Summary, 
                options.IssueType, 
                description);
            
            Console.WriteLine();
            Console.WriteLine("Issue created successfully!");
            Console.WriteLine($"  Key:  {response.Key}");
            Console.WriteLine($"  ID:   {response.Id}");
            if (response.Self != null)
            {
                Console.WriteLine($"  URL:  {response.Self}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the add-comment command.
    /// </summary>
    public static async Task<int> HandleAddCommentAsync(AddCommentOptions options)
    {
        try
        {
            string body = ResolveContent(options.Body, options.FilePath, "body");

            using var client = new JiraClient();

            Console.WriteLine($"Adding comment to issue '{options.IssueKey}'...");
            
            var comment = await client.AddCommentAsync(options.IssueKey, body);
            
            Console.WriteLine();
            Console.WriteLine("Comment added successfully!");
            Console.WriteLine($"  Comment ID: {comment.Id}");
            Console.WriteLine($"  Created:    {comment.Created}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the change-status command.
    /// </summary>
    public static async Task<int> HandleChangeStatusAsync(ChangeStatusOptions options)
    {
        try
        {
            using var client = new JiraClient();

            Console.WriteLine($"Changing status of issue '{options.IssueKey}' to '{options.Status}'...");
            
            await client.TransitionIssueAsync(options.IssueKey, options.Status);
            
            Console.WriteLine();
            Console.WriteLine($"Issue '{options.IssueKey}' transitioned to '{options.Status}' successfully!");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the assign-user command.
    /// </summary>
    public static async Task<int> HandleAssignUserAsync(AssignUserOptions options)
    {
        try
        {
            using var client = new JiraClient();

            Console.WriteLine($"Assigning user '{options.User}' to issue '{options.IssueKey}'...");
            
            await client.AssignIssueAsync(options.IssueKey, options.User);
            
            Console.WriteLine();
            Console.WriteLine($"User '{options.User}' assigned to issue '{options.IssueKey}' successfully!");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the update-issue command.
    /// </summary>
    public static async Task<int> HandleUpdateIssueAsync(UpdateIssueOptions options)
    {
        try
        {
            string description = ResolveContent(options.Description, options.FilePath, "description");

            using var client = new JiraClient();

            Console.WriteLine($"Updating description of issue '{options.IssueKey}'...");
            
            await client.UpdateIssueDescriptionAsync(options.IssueKey, description);
            
            Console.WriteLine();
            Console.WriteLine($"Issue '{options.IssueKey}' description updated successfully!");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Displays issue information to the console.
    /// </summary>
    private static void DisplayIssue(JiraIssue issue)
    {
        Console.WriteLine("=== Issue Information ===");
        Console.WriteLine($"Key:         {issue.Key}");
        Console.WriteLine($"ID:          {issue.Id}");
        Console.WriteLine($"Summary:     {issue.Fields.Summary}");
        Console.WriteLine($"Type:        {issue.Fields.IssueType?.Name ?? "N/A"}");
        Console.WriteLine($"Status:      {issue.Fields.Status?.Name ?? "N/A"}");
        Console.WriteLine($"Project:     {issue.Fields.Project?.Key ?? "N/A"} ({issue.Fields.Project?.Name ?? "N/A"})");
        Console.WriteLine($"Assignee:    {issue.Fields.Assignee?.DisplayName ?? "Unassigned"}");
        Console.WriteLine($"Reporter:    {issue.Fields.Reporter?.DisplayName ?? "N/A"}");
        Console.WriteLine($"Priority:    {issue.Fields.Priority?.Name ?? "N/A"}");
        Console.WriteLine($"Created:     {issue.Fields.Created}");
        Console.WriteLine($"Updated:     {issue.Fields.Updated}");
        
        if (issue.Self != null)
        {
            Console.WriteLine($"URL:         {issue.Self}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Description ===");
        if (!string.IsNullOrEmpty(issue.Fields.Description))
        {
            Console.WriteLine(issue.Fields.Description);
        }
        else
        {
            Console.WriteLine("(No description)");
        }

        if (issue.Fields.Comment?.Comments != null && issue.Fields.Comment.Comments.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Comments ({issue.Fields.Comment.Total}) ===");
            foreach (var comment in issue.Fields.Comment.Comments.Take(MaxDisplayedComments))
            {
                Console.WriteLine($"[{comment.Created}] {comment.Author?.DisplayName ?? "Unknown"}:");
                Console.WriteLine($"  {comment.Body}");
                Console.WriteLine();
            }
            if (issue.Fields.Comment.Total > MaxDisplayedComments)
            {
                Console.WriteLine($"... and {issue.Fields.Comment.Total - MaxDisplayedComments} more comments");
            }
        }
    }
}
