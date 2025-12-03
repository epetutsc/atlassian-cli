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
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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
                await Console.Error.WriteLineAsync("Error: You must provide either --id or both --space and --title.");
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
                await Console.Error.WriteLineAsync("Page not found.");
                return 1;
            }

            Console.WriteLine();
            DisplayPage(page, options.Format);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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
                await Console.Error.WriteLineAsync("Error: You must provide either --id or both --space and --title.");
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
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
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

    // ==================== Bitbucket Handlers ====================

    /// <summary>
    /// Handles the get-repo command.
    /// </summary>
    public static async Task<int> HandleGetRepoAsync(GetRepoOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Fetching repository '{options.ProjectKey}/{options.RepoSlug}'...");

            var repo = await client.GetRepositoryAsync(options.ProjectKey, options.RepoSlug);

            Console.WriteLine();
            DisplayRepository(repo);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the list-repos command.
    /// </summary>
    public static async Task<int> HandleListReposAsync(ListReposOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Listing repositories in project '{options.ProjectKey}'...");

            var result = await client.ListRepositoriesAsync(options.ProjectKey, options.Limit);

            Console.WriteLine();
            Console.WriteLine($"=== Repositories ({result.Size}) ===");
            foreach (var repo in result.Values)
            {
                Console.WriteLine($"  - {repo.Slug}: {repo.Name}");
                if (!string.IsNullOrEmpty(repo.Description))
                {
                    Console.WriteLine($"    {repo.Description}");
                }
            }

            if (!result.IsLastPage)
            {
                Console.WriteLine($"\n(More results available)");
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
    /// Handles the list-branches command.
    /// </summary>
    public static async Task<int> HandleListBranchesAsync(ListBranchesOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Listing branches in '{options.ProjectKey}/{options.RepoSlug}'...");

            var result = await client.ListBranchesAsync(options.ProjectKey, options.RepoSlug, options.Limit);

            Console.WriteLine();
            Console.WriteLine($"=== Branches ({result.Size}) ===");
            foreach (var branch in result.Values)
            {
                var defaultMarker = branch.IsDefault ? " (default)" : "";
                Console.WriteLine($"  - {branch.DisplayId}{defaultMarker}");
                if (!string.IsNullOrEmpty(branch.LatestCommit))
                {
                    Console.WriteLine($"    Latest commit: {branch.LatestCommit[..Math.Min(8, branch.LatestCommit.Length)]}");
                }
            }

            if (!result.IsLastPage)
            {
                Console.WriteLine($"\n(More results available)");
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
    /// Handles the list-commits command.
    /// </summary>
    public static async Task<int> HandleListCommitsAsync(ListCommitsOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            var branchInfo = string.IsNullOrEmpty(options.Branch) ? "" : $" on branch '{options.Branch}'";
            Console.WriteLine($"Listing commits in '{options.ProjectKey}/{options.RepoSlug}'{branchInfo}...");

            var result = await client.ListCommitsAsync(options.ProjectKey, options.RepoSlug, options.Branch, options.Limit);

            Console.WriteLine();
            Console.WriteLine($"=== Commits ({result.Size}) ===");
            foreach (var commit in result.Values)
            {
                string shortId;
                if (!string.IsNullOrEmpty(commit.DisplayId))
                {
                    shortId = commit.DisplayId;
                }
                else if (!string.IsNullOrEmpty(commit.Id))
                {
                    shortId = commit.Id[..Math.Min(8, commit.Id.Length)];
                }
                else
                {
                    shortId = "unknown";
                }

                var message = !string.IsNullOrEmpty(commit.Message) ? commit.Message.Split('\n')[0] : "(no message)";
                if (message.Length > 60) message = message[..57] + "...";

                Console.WriteLine($"  {shortId} - {message}");
                if (commit.Author != null)
                {
                    Console.WriteLine($"           by {commit.Author.Name}");
                }
            }

            if (!result.IsLastPage)
            {
                Console.WriteLine($"\n(More results available)");
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
    /// Handles the get-commit command.
    /// </summary>
    public static async Task<int> HandleGetCommitAsync(GetCommitOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Fetching commit '{options.CommitId}' in '{options.ProjectKey}/{options.RepoSlug}'...");

            var commit = await client.GetCommitAsync(options.ProjectKey, options.RepoSlug, options.CommitId);

            Console.WriteLine();
            DisplayCommit(commit);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the list-pull-requests command.
    /// </summary>
    public static async Task<int> HandleListPullRequestsAsync(ListPullRequestsOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Listing {options.State} pull requests in '{options.ProjectKey}/{options.RepoSlug}'...");

            var result = await client.ListPullRequestsAsync(options.ProjectKey, options.RepoSlug, options.State, options.Limit);

            Console.WriteLine();
            Console.WriteLine($"=== Pull Requests ({result.Size}) ===");
            foreach (var pr in result.Values)
            {
                Console.WriteLine($"  #{pr.Id} [{pr.State}] {pr.Title}");
                if (pr.FromRef != null && pr.ToRef != null)
                {
                    Console.WriteLine($"         {pr.FromRef.DisplayId} -> {pr.ToRef.DisplayId}");
                }
                if (pr.Author?.User != null)
                {
                    Console.WriteLine($"         by {pr.Author.User.DisplayName}");
                }
            }

            if (!result.IsLastPage)
            {
                Console.WriteLine($"\n(More results available)");
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
    /// Handles the get-pull-request command.
    /// </summary>
    public static async Task<int> HandleGetPullRequestAsync(GetPullRequestOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Fetching pull request #{options.PullRequestId} in '{options.ProjectKey}/{options.RepoSlug}'...");

            var pr = await client.GetPullRequestAsync(options.ProjectKey, options.RepoSlug, options.PullRequestId);

            Console.WriteLine();
            DisplayPullRequest(pr);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the get-project command.
    /// </summary>
    public static async Task<int> HandleGetProjectAsync(GetProjectOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Fetching project '{options.ProjectKey}'...");

            var project = await client.GetProjectAsync(options.ProjectKey);

            Console.WriteLine();
            DisplayProject(project);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the list-projects command.
    /// </summary>
    public static async Task<int> HandleListProjectsAsync(ListProjectsOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine("Listing projects...");

            var result = await client.ListProjectsAsync(options.Limit);

            Console.WriteLine();
            Console.WriteLine($"=== Projects ({result.Size}) ===");
            foreach (var project in result.Values)
            {
                Console.WriteLine($"  - {project.Key}: {project.Name}");
                if (!string.IsNullOrEmpty(project.Description))
                {
                    Console.WriteLine($"    {project.Description}");
                }
            }

            if (!result.IsLastPage)
            {
                Console.WriteLine($"\n(More results available)");
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
    /// Handles the get-webhooks command.
    /// </summary>
    public static async Task<int> HandleGetWebhooksAsync(GetWebhooksOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Fetching webhooks for '{options.ProjectKey}/{options.RepoSlug}'...");

            var result = await client.GetWebhooksAsync(options.ProjectKey, options.RepoSlug);

            Console.WriteLine();
            Console.WriteLine($"=== Webhooks ({result.Size}) ===");
            foreach (var webhook in result.Values)
            {
                var status = webhook.Active ? "active" : "inactive";
                Console.WriteLine($"  - {webhook.Name} [{status}]");
                Console.WriteLine($"    URL: {webhook.Url}");
                if (webhook.Events.Count > 0)
                {
                    Console.WriteLine($"    Events: {string.Join(", ", webhook.Events)}");
                }
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
    /// Handles the get-branch-restrictions command.
    /// </summary>
    public static async Task<int> HandleGetBranchRestrictionsAsync(GetBranchRestrictionsOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Fetching branch restrictions for '{options.ProjectKey}/{options.RepoSlug}'...");

            var result = await client.GetBranchRestrictionsAsync(options.ProjectKey, options.RepoSlug);

            Console.WriteLine();
            Console.WriteLine($"=== Branch Restrictions ({result.Size}) ===");
            foreach (var restriction in result.Values)
            {
                Console.WriteLine($"  - Type: {restriction.Type}");
                if (restriction.Matcher != null)
                {
                    Console.WriteLine($"    Pattern: {restriction.Matcher.DisplayId}");
                }
                if (restriction.Users != null && restriction.Users.Count > 0)
                {
                    Console.WriteLine($"    Users: {string.Join(", ", restriction.Users.Select(u => u.DisplayName))}");
                }
                if (restriction.Groups != null && restriction.Groups.Count > 0)
                {
                    Console.WriteLine($"    Groups: {string.Join(", ", restriction.Groups)}");
                }
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
    /// Handles the list-pipelines command.
    /// </summary>
    public static async Task<int> HandleListPipelinesAsync(ListPipelinesOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Listing pipelines for '{options.ProjectKey}/{options.RepoSlug}'...");

            var result = await client.ListPipelinesAsync(options.ProjectKey, options.RepoSlug, options.Limit);

            Console.WriteLine();
            Console.WriteLine($"=== Pipelines ({result.Size}) ===");
            foreach (var pipeline in result.Values)
            {
                var state = pipeline.State?.Name ?? "Unknown";
                var result_name = pipeline.State?.Result?.Name ?? "";
                var status = !string.IsNullOrEmpty(result_name) ? $"{state} ({result_name})" : state;

                Console.WriteLine($"  #{pipeline.BuildNumber} [{status}]");
                if (pipeline.Target != null)
                {
                    Console.WriteLine($"    Branch: {pipeline.Target.RefName}");
                }
                if (!string.IsNullOrEmpty(pipeline.CreatedOn))
                {
                    Console.WriteLine($"    Created: {pipeline.CreatedOn}");
                }
                if (pipeline.DurationInSeconds.HasValue)
                {
                    Console.WriteLine($"    Duration: {pipeline.DurationInSeconds}s");
                }
            }

            if (!result.IsLastPage)
            {
                Console.WriteLine($"\n(More results available)");
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
    /// Handles the get-pipeline command.
    /// </summary>
    public static async Task<int> HandleGetPipelineAsync(GetPipelineOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Fetching pipeline '{options.PipelineUuid}' in '{options.ProjectKey}/{options.RepoSlug}'...");

            var pipeline = await client.GetPipelineAsync(options.ProjectKey, options.RepoSlug, options.PipelineUuid);

            Console.WriteLine();
            DisplayPipeline(pipeline);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the trigger-pipeline command.
    /// </summary>
    public static async Task<int> HandleTriggerPipelineAsync(TriggerPipelineOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            var customInfo = string.IsNullOrEmpty(options.CustomPipeline) ? "" : $" (custom: {options.CustomPipeline})";
            Console.WriteLine($"Triggering pipeline on branch '{options.Branch}'{customInfo} for '{options.ProjectKey}/{options.RepoSlug}'...");

            var pipeline = await client.TriggerPipelineAsync(options.ProjectKey, options.RepoSlug, options.Branch, options.CustomPipeline);

            Console.WriteLine();
            Console.WriteLine("Pipeline triggered successfully!");
            Console.WriteLine($"  Build Number: {pipeline.BuildNumber}");
            Console.WriteLine($"  UUID:         {pipeline.Uuid}");
            if (pipeline.State != null)
            {
                Console.WriteLine($"  State:        {pipeline.State.Name}");
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
    /// Handles the stop-pipeline command.
    /// </summary>
    public static async Task<int> HandleStopPipelineAsync(StopPipelineOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Stopping pipeline '{options.PipelineUuid}' for '{options.ProjectKey}/{options.RepoSlug}'...");

            await client.StopPipelineAsync(options.ProjectKey, options.RepoSlug, options.PipelineUuid);

            Console.WriteLine();
            Console.WriteLine("Pipeline stopped successfully!");

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the get-build-status command.
    /// </summary>
    public static async Task<int> HandleGetBuildStatusAsync(GetBuildStatusOptions options)
    {
        try
        {
            using var client = new BitbucketClient();

            Console.WriteLine($"Fetching build statuses for commit '{options.CommitId}' in '{options.ProjectKey}/{options.RepoSlug}'...");

            var result = await client.GetBuildStatusesAsync(options.ProjectKey, options.RepoSlug, options.CommitId);

            Console.WriteLine();
            Console.WriteLine($"=== Build Statuses ({result.Size}) ===");
            foreach (var status in result.Values)
            {
                Console.WriteLine($"  - {status.Key}: {status.State}");
                if (!string.IsNullOrEmpty(status.Name))
                {
                    Console.WriteLine($"    Name: {status.Name}");
                }
                if (!string.IsNullOrEmpty(status.Url))
                {
                    Console.WriteLine($"    URL:  {status.Url}");
                }
                if (!string.IsNullOrEmpty(status.Description))
                {
                    Console.WriteLine($"    Description: {status.Description}");
                }
            }

            if (result.Size == 0)
            {
                Console.WriteLine("  (No build statuses found)");
            }

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    // ==================== Bitbucket Display Methods ====================

    /// <summary>
    /// Displays repository information to the console.
    /// </summary>
    private static void DisplayRepository(BitbucketRepository repo)
    {
        Console.WriteLine("=== Repository Information ===");
        Console.WriteLine($"Slug:        {repo.Slug}");
        Console.WriteLine($"Name:        {repo.Name}");
        Console.WriteLine($"Description: {repo.Description ?? "(none)"}");
        Console.WriteLine($"SCM:         {repo.ScmId}");
        Console.WriteLine($"State:       {repo.State ?? "N/A"}");
        Console.WriteLine($"Forkable:    {repo.Forkable}");
        Console.WriteLine($"Public:      {repo.IsPublic}");

        if (repo.Project != null)
        {
            Console.WriteLine($"Project:     {repo.Project.Key} ({repo.Project.Name})");
        }

        if (repo.Links?.Clone != null && repo.Links.Clone.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Clone URLs:");
            foreach (var link in repo.Links.Clone)
            {
                Console.WriteLine($"  {link.Name}: {link.Href}");
            }
        }
    }

    /// <summary>
    /// Displays commit information to the console.
    /// </summary>
    private static void DisplayCommit(BitbucketCommit commit)
    {
        Console.WriteLine("=== Commit Information ===");
        Console.WriteLine($"ID:        {commit.Id}");
        Console.WriteLine($"Short ID:  {commit.DisplayId}");
        Console.WriteLine($"Message:   {commit.Message}");

        if (commit.Author != null)
        {
            Console.WriteLine($"Author:    {commit.Author.Name} <{commit.Author.EmailAddress ?? "N/A"}>");
        }

        if (commit.Committer != null && commit.Committer.Name != commit.Author?.Name)
        {
            Console.WriteLine($"Committer: {commit.Committer.Name} <{commit.Committer.EmailAddress ?? "N/A"}>");
        }

        if (commit.Parents != null && commit.Parents.Count > 0)
        {
            var parentIds = string.Join(", ", commit.Parents.Select(p => p.DisplayId));
            Console.WriteLine($"Parents:   {parentIds}");
        }
    }

    /// <summary>
    /// Displays pull request information to the console.
    /// </summary>
    private static void DisplayPullRequest(BitbucketPullRequest pr)
    {
        Console.WriteLine("=== Pull Request Information ===");
        Console.WriteLine($"ID:          #{pr.Id}");
        Console.WriteLine($"Title:       {pr.Title}");
        Console.WriteLine($"State:       {pr.State}");
        Console.WriteLine($"Description: {pr.Description ?? "(none)"}");

        if (pr.FromRef != null)
        {
            Console.WriteLine($"From:        {pr.FromRef.DisplayId}");
        }
        if (pr.ToRef != null)
        {
            Console.WriteLine($"To:          {pr.ToRef.DisplayId}");
        }

        if (pr.Author?.User != null)
        {
            Console.WriteLine($"Author:      {pr.Author.User.DisplayName}");
        }

        if (pr.Reviewers != null && pr.Reviewers.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Reviewers:");
            foreach (var reviewer in pr.Reviewers)
            {
                var status = reviewer.Approved ? "approved" : (reviewer.Status ?? "pending");
                Console.WriteLine($"  - {reviewer.User?.DisplayName ?? "Unknown"}: {status}");
            }
        }
    }

    /// <summary>
    /// Displays project information to the console.
    /// </summary>
    private static void DisplayProject(BitbucketProject project)
    {
        Console.WriteLine("=== Project Information ===");
        Console.WriteLine($"Key:         {project.Key}");
        Console.WriteLine($"Name:        {project.Name}");
        Console.WriteLine($"Description: {project.Description ?? "(none)"}");
        Console.WriteLine($"Public:      {project.IsPublic}");
        Console.WriteLine($"Type:        {project.Type ?? "N/A"}");
    }

    /// <summary>
    /// Displays pipeline information to the console.
    /// </summary>
    private static void DisplayPipeline(BitbucketPipeline pipeline)
    {
        Console.WriteLine("=== Pipeline Information ===");
        Console.WriteLine($"Build #:     {pipeline.BuildNumber}");
        Console.WriteLine($"UUID:        {pipeline.Uuid}");

        if (pipeline.State != null)
        {
            var result = pipeline.State.Result?.Name ?? "";
            var status = !string.IsNullOrEmpty(result) ? $"{pipeline.State.Name} ({result})" : pipeline.State.Name;
            Console.WriteLine($"State:       {status}");
        }

        if (pipeline.Target != null)
        {
            Console.WriteLine($"Branch:      {pipeline.Target.RefName}");
            if (pipeline.Target.Commit != null)
            {
                Console.WriteLine($"Commit:      {pipeline.Target.Commit.Hash}");
            }
        }

        if (pipeline.Trigger != null)
        {
            Console.WriteLine($"Trigger:     {pipeline.Trigger.Type}");
        }

        if (!string.IsNullOrEmpty(pipeline.CreatedOn))
        {
            Console.WriteLine($"Created:     {pipeline.CreatedOn}");
        }
        if (!string.IsNullOrEmpty(pipeline.CompletedOn))
        {
            Console.WriteLine($"Completed:   {pipeline.CompletedOn}");
        }
        if (pipeline.DurationInSeconds.HasValue)
        {
            Console.WriteLine($"Duration:    {pipeline.DurationInSeconds}s");
        }
    }
}
