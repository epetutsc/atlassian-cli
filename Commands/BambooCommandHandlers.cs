using AtlassianCli.Client;
using AtlassianCli.Models;
using System.Text.RegularExpressions;

namespace AtlassianCli.Commands;

/// <summary>
/// Handles execution of CLI commands for Bamboo operations.
/// </summary>
public static class BambooCommandHandlers
{
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

    /// <summary>
    /// Handles the get-build-logs command for Bamboo.
    /// </summary>
    public static async Task<int> HandleGetBambooBuildLogsAsync(GetBambooBuildLogsOptions options)
    {
        try
        {
            using var client = new BambooClient();

            string logs;
            if (!string.IsNullOrEmpty(options.JobKey))
            {
                Console.WriteLine($"Fetching job logs for '{options.JobKey}' in build '{options.BuildKey}'...");
                logs = await client.GetJobLogsAsync(options.BuildKey, options.JobKey);
            }
            else
            {
                Console.WriteLine($"Fetching build logs for '{options.BuildKey}'...");
                logs = await client.GetBuildLogsAsync(options.BuildKey);
            }

            Console.WriteLine();

            // Apply filters if specified
            var filters = options.Filters.Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
            if (filters.Count > 0)
            {
                // Compile regex patterns with case-insensitive matching
                List<Regex> regexPatterns;
                try
                {
                    regexPatterns = filters
                        .Select(f => new Regex(f, RegexOptions.IgnoreCase | RegexOptions.Compiled))
                        .ToList();
                }
                catch (ArgumentException ex)
                {
                    await Console.Error.WriteLineAsync($"Error: Invalid regex pattern - {ex.Message}");
                    return 1;
                }

                var filteredLines = logs
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => regexPatterns.Any(regex => regex.IsMatch(line)))
                    .ToList();

                var filterDescription = string.Join(", ", filters.Select(f => $"'{f}'"));
                if (filteredLines.Count == 0)
                {
                    Console.WriteLine($"No log lines found matching filter(s): {filterDescription}");
                }
                else
                {
                    Console.WriteLine($"=== Log Lines Matching {filterDescription} ({filteredLines.Count} matches) ===");
                    foreach (var line in filteredLines)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            else
            {
                Console.WriteLine("=== Build Logs ===");
                Console.WriteLine(logs);
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
