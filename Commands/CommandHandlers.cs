using AtlassianCli.Client;
using AtlassianCli.Models;
using System.Text;

namespace AtlassianCli.Commands;

/// <summary>
/// Handles execution of CLI commands for Confluence, Jira, and Bamboo operations.
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

    // ==================== Bamboo Handlers ====================

    /// <summary>
    /// Handles the get-projects command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooProjectsAsync(GetBambooProjectsOptions options)
    {
        _ = options; // Suppress unused parameter warning
        try
        {
            using var client = new BambooClient();

            Console.WriteLine("Fetching Bamboo projects...");
            
            var projects = await client.GetProjectsAsync();
            
            Console.WriteLine();
            Console.WriteLine($"=== Bamboo Projects ({projects.Count}) ===");
            foreach (var project in projects)
            {
                Console.WriteLine($"  {project.Key,-15} {project.Name}");
                if (!string.IsNullOrEmpty(project.Description))
                {
                    Console.WriteLine($"                  {project.Description}");
                }
                if (project.Plans?.Plan != null && project.Plans.Plan.Count > 0)
                {
                    Console.WriteLine($"                  Plans: {project.Plans.Plan.Count}");
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
    /// Handles the get-project command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooProjectAsync(GetBambooProjectOptions options)
    {
        try
        {
            using var client = new BambooClient();

            Console.WriteLine($"Fetching project '{options.ProjectKey}'...");
            
            var project = await client.GetProjectAsync(options.ProjectKey);
            
            Console.WriteLine();
            DisplayBambooProject(project);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the get-plans command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooPlansAsync(GetBambooPlansOptions options)
    {
        try
        {
            using var client = new BambooClient();

            Console.WriteLine("Fetching Bamboo plans...");
            
            var plans = await client.GetPlansAsync();
            
            // Filter by project if specified
            if (!string.IsNullOrEmpty(options.ProjectKey))
            {
                plans = plans.Where(p => 
                    string.Equals(p.ProjectKey, options.ProjectKey, StringComparison.OrdinalIgnoreCase) ||
                    p.Key.StartsWith(options.ProjectKey + "-", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            Console.WriteLine();
            Console.WriteLine($"=== Bamboo Plans ({plans.Count}) ===");
            foreach (var plan in plans)
            {
                var status = plan.Enabled ? "Enabled" : "Disabled";
                var building = plan.IsBuilding ? " [BUILDING]" : "";
                Console.WriteLine($"  {plan.Key,-25} {plan.Name} ({status}){building}");
                if (!string.IsNullOrEmpty(plan.ProjectName))
                {
                    Console.WriteLine($"                           Project: {plan.ProjectName}");
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
    /// Handles the get-plan command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooPlanAsync(GetBambooPlanOptions options)
    {
        try
        {
            using var client = new BambooClient();

            Console.WriteLine($"Fetching plan '{options.PlanKey}'...");
            
            var plan = await client.GetPlanAsync(options.PlanKey);
            
            Console.WriteLine();
            DisplayBambooPlan(plan);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the get-branches command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooBranchesAsync(GetBambooBranchesOptions options)
    {
        try
        {
            using var client = new BambooClient();

            Console.WriteLine($"Fetching branches for plan '{options.PlanKey}'...");
            
            var branches = await client.GetPlanBranchesAsync(options.PlanKey);
            
            Console.WriteLine();
            Console.WriteLine($"=== Branches for {options.PlanKey} ({branches.Count}) ===");
            foreach (var branch in branches)
            {
                var status = branch.Enabled ? "Enabled" : "Disabled";
                Console.WriteLine($"  {branch.Key,-30} {branch.Name} ({status})");
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
    /// Handles the get-builds command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooBuildsAsync(GetBambooBuildsOptions options)
    {
        try
        {
            using var client = new BambooClient();

            Console.WriteLine($"Fetching build results for plan '{options.PlanKey}'...");
            
            var builds = await client.GetBuildResultsAsync(options.PlanKey, options.MaxResults);
            
            Console.WriteLine();
            Console.WriteLine($"=== Build Results for {options.PlanKey} ({builds.Count}) ===");
            foreach (var build in builds)
            {
                var status = build.Successful ? "SUCCESS" : "FAILED";
                var state = build.Finished ? status : build.LifeCycleState ?? "UNKNOWN";
                Console.WriteLine($"  #{build.BuildNumber,-6} {build.Key,-25} {state,-10} {build.BuildRelativeTime ?? ""}");
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
    /// Handles the get-build command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooBuildAsync(GetBambooBuildOptions options)
    {
        try
        {
            using var client = new BambooClient();

            Console.WriteLine($"Fetching build result '{options.BuildKey}'...");
            
            var build = await client.GetBuildResultAsync(options.BuildKey);
            
            Console.WriteLine();
            DisplayBambooBuildResult(build);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the get-latest-build command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooLatestBuildAsync(GetBambooLatestBuildOptions options)
    {
        try
        {
            using var client = new BambooClient();

            Console.WriteLine($"Fetching latest build result for plan '{options.PlanKey}'...");
            
            var build = await client.GetLatestBuildResultAsync(options.PlanKey);
            
            Console.WriteLine();
            DisplayBambooBuildResult(build);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the queue-build command for Bamboo.
    /// </summary>
    public static async Task<int> HandleQueueBambooBuildAsync(QueueBambooBuildOptions options)
    {
        try
        {
            using var client = new BambooClient();

            BambooQueueResponse response;
            if (!string.IsNullOrEmpty(options.Branch))
            {
                Console.WriteLine($"Queuing build for plan '{options.PlanKey}' branch '{options.Branch}'...");
                response = await client.QueueBranchBuildAsync(options.PlanKey, options.Branch);
            }
            else
            {
                Console.WriteLine($"Queuing build for plan '{options.PlanKey}'...");
                response = await client.QueueBuildAsync(options.PlanKey);
            }
            
            Console.WriteLine();
            Console.WriteLine("Build queued successfully!");
            Console.WriteLine($"  Build Number: {response.BuildNumber}");
            if (!string.IsNullOrEmpty(response.BuildResultKey))
            {
                Console.WriteLine($"  Build Key:    {response.BuildResultKey}");
            }
            if (!string.IsNullOrEmpty(response.TriggerReason))
            {
                Console.WriteLine($"  Trigger:      {response.TriggerReason}");
            }
            if (response.Link != null)
            {
                Console.WriteLine($"  URL:          {response.Link.Href}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    // ==================== Bamboo Display Helpers ====================

    /// <summary>
    /// Displays Bamboo project information to the console.
    /// </summary>
    private static void DisplayBambooProject(BambooProject project)
    {
        Console.WriteLine("=== Project Information ===");
        Console.WriteLine($"Key:         {project.Key}");
        Console.WriteLine($"Name:        {project.Name}");
        if (!string.IsNullOrEmpty(project.Description))
        {
            Console.WriteLine($"Description: {project.Description}");
        }
        if (project.Link != null)
        {
            Console.WriteLine($"URL:         {project.Link.Href}");
        }

        if (project.Plans?.Plan != null && project.Plans.Plan.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Plans ({project.Plans.Plan.Count}) ===");
            foreach (var plan in project.Plans.Plan)
            {
                var status = plan.Enabled ? "Enabled" : "Disabled";
                Console.WriteLine($"  {plan.Key,-25} {plan.Name} ({status})");
            }
        }
    }

    /// <summary>
    /// Displays Bamboo plan information to the console.
    /// </summary>
    private static void DisplayBambooPlan(BambooPlan plan)
    {
        Console.WriteLine("=== Plan Information ===");
        Console.WriteLine($"Key:           {plan.Key}");
        Console.WriteLine($"Name:          {plan.Name}");
        Console.WriteLine($"Project:       {plan.ProjectName ?? plan.ProjectKey ?? "N/A"}");
        Console.WriteLine($"Enabled:       {plan.Enabled}");
        Console.WriteLine($"Type:          {plan.Type ?? "N/A"}");
        Console.WriteLine($"Is Building:   {plan.IsBuilding}");
        Console.WriteLine($"Is Active:     {plan.IsActive}");
        Console.WriteLine($"Is Favourite:  {plan.IsFavourite}");
        if (plan.AverageBuildTimeInSeconds.HasValue)
        {
            Console.WriteLine($"Avg Build Time: {plan.AverageBuildTimeInSeconds:F0} seconds");
        }
        if (!string.IsNullOrEmpty(plan.Description))
        {
            Console.WriteLine($"Description:   {plan.Description}");
        }
        if (plan.Link != null)
        {
            Console.WriteLine($"URL:           {plan.Link.Href}");
        }

        // Display stages
        if (plan.Stages?.Stage != null && plan.Stages.Stage.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Stages ({plan.Stages.Stage.Count}) ===");
            foreach (var stage in plan.Stages.Stage)
            {
                Console.WriteLine($"  {stage.Name}");
                if (!string.IsNullOrEmpty(stage.Description))
                {
                    Console.WriteLine($"    {stage.Description}");
                }
            }
        }

        // Display variables
        if (plan.VariableContext?.Variable != null && plan.VariableContext.Variable.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Variables ({plan.VariableContext.Variable.Count}) ===");
            foreach (var variable in plan.VariableContext.Variable)
            {
                Console.WriteLine($"  {variable.Name}: {variable.Value ?? "(not set)"}");
            }
        }

        // Display branches summary
        if (plan.Branches?.Branch != null && plan.Branches.Branch.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Branches ({plan.Branches.Branch.Count}) ===");
            foreach (var branch in plan.Branches.Branch.Take(10))
            {
                var status = branch.Enabled ? "Enabled" : "Disabled";
                Console.WriteLine($"  {branch.Key,-30} {branch.Name} ({status})");
            }
            if (plan.Branches.Branch.Count > 10)
            {
                Console.WriteLine($"  ... and {plan.Branches.Branch.Count - 10} more branches");
            }
        }
    }

    /// <summary>
    /// Displays Bamboo build result information to the console.
    /// </summary>
    private static void DisplayBambooBuildResult(BambooBuildResult build)
    {
        var status = build.Successful ? "SUCCESS" : "FAILED";
        var state = build.Finished ? status : (build.LifeCycleState ?? "UNKNOWN");

        Console.WriteLine("=== Build Result ===");
        Console.WriteLine($"Key:           {build.Key}");
        Console.WriteLine($"Build Number:  {build.BuildNumber}");
        Console.WriteLine($"State:         {state}");
        Console.WriteLine($"Successful:    {build.Successful}");
        Console.WriteLine($"Finished:      {build.Finished}");
        Console.WriteLine($"Plan:          {build.PlanName ?? build.Plan?.Name ?? "N/A"}");
        Console.WriteLine($"Project:       {build.ProjectName ?? build.Plan?.ProjectName ?? "N/A"}");
        if (!string.IsNullOrEmpty(build.BuildStartedTime))
        {
            Console.WriteLine($"Started:       {build.BuildStartedTime}");
        }
        if (!string.IsNullOrEmpty(build.BuildCompletedTime))
        {
            Console.WriteLine($"Completed:     {build.BuildCompletedTime}");
        }
        if (!string.IsNullOrEmpty(build.BuildDurationDescription))
        {
            Console.WriteLine($"Duration:      {build.BuildDurationDescription}");
        }
        if (!string.IsNullOrEmpty(build.BuildReason))
        {
            Console.WriteLine($"Reason:        {build.BuildReason}");
        }
        if (!string.IsNullOrEmpty(build.ReasonSummary))
        {
            Console.WriteLine($"Summary:       {build.ReasonSummary}");
        }
        if (build.Link != null)
        {
            Console.WriteLine($"URL:           {build.Link.Href}");
        }

        // Test results
        if (build.SuccessfulTestCount > 0 || build.FailedTestCount > 0)
        {
            Console.WriteLine();
            Console.WriteLine("=== Test Results ===");
            Console.WriteLine($"Successful:    {build.SuccessfulTestCount}");
            Console.WriteLine($"Failed:        {build.FailedTestCount}");
            Console.WriteLine($"Quarantined:   {build.QuarantinedTestCount}");
            Console.WriteLine($"Skipped:       {build.SkippedTestCount}");
        }

        // Stage results
        if (build.Stages?.Stage != null && build.Stages.Stage.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Stage Results ({build.Stages.Stage.Count}) ===");
            foreach (var stage in build.Stages.Stage)
            {
                var stageStatus = stage.Successful ? "SUCCESS" : "FAILED";
                var stageState = stage.Finished ? stageStatus : (stage.State ?? "UNKNOWN");
                Console.WriteLine($"  {stage.Name,-20} {stageState}");
            }
        }

        // Changes
        if (build.Changes?.Change != null && build.Changes.Change.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Changes ({build.Changes.Change.Count}) ===");
            foreach (var change in build.Changes.Change.Take(5))
            {
                var author = change.FullName ?? change.Author ?? change.UserName ?? "Unknown";
                Console.WriteLine($"  [{change.ChangesetId?.Substring(0, Math.Min(8, change.ChangesetId.Length)) ?? "N/A"}] {author}");
                if (!string.IsNullOrEmpty(change.Comment))
                {
                    var comment = change.Comment.Length > 80 
                        ? change.Comment.Substring(0, 77) + "..." 
                        : change.Comment;
                    Console.WriteLine($"    {comment}");
                }
            }
            if (build.Changes.Change.Count > 5)
            {
                Console.WriteLine($"  ... and {build.Changes.Change.Count - 5} more changes");
            }
        }
    }
}
