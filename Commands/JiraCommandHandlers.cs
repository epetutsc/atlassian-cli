using AtlassianCli.Client;
using AtlassianCli.Models;

namespace AtlassianCli.Commands;

/// <summary>
/// Handles execution of CLI commands for Jira operations.
/// </summary>
public static class JiraCommandHandlers
{
    /// <summary>
    /// Maximum number of comments to display when showing issue details.
    /// </summary>
    private const int MaxDisplayedComments = 5;

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
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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
            string? description = ContentResolver.ResolveOptionalContent(options.Description, options.DescriptionFilePath, "description", "description-file");

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
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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
            string body = ContentResolver.ResolveContent(options.Body, options.FilePath, "body");

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
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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
            string description = ContentResolver.ResolveContent(options.Description, options.FilePath, "description");

            using var client = new JiraClient();

            Console.WriteLine($"Updating description of issue '{options.IssueKey}'...");
            
            await client.UpdateIssueDescriptionAsync(options.IssueKey, description);
            
            Console.WriteLine();
            Console.WriteLine($"Issue '{options.IssueKey}' description updated successfully!");

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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
