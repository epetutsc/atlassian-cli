using AtlassianCli.Client;
using AtlassianCli.Models;

namespace AtlassianCli.Commands;

/// <summary>
/// Handles execution of CLI commands for BitBucket operations.
/// </summary>
public static class BitBucketCommandHandlers
{
    /// <summary>
    /// Handles the get-pr command.
    /// </summary>
    public static async Task<int> HandleGetPullRequestAsync(GetPullRequestOptions options)
    {
        try
        {
            using var client = new BitBucketClient();

            Console.WriteLine($"Fetching pull request {options.PullRequestId} from {options.ProjectKey}/{options.RepositorySlug}...");
            
            var pr = await client.GetPullRequestAsync(options.ProjectKey, options.RepositorySlug, options.PullRequestId);
            
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
    /// Handles the get-pr-diff command.
    /// </summary>
    public static async Task<int> HandleGetPullRequestDiffAsync(GetPullRequestDiffOptions options)
    {
        try
        {
            using var client = new BitBucketClient();

            Console.WriteLine($"Fetching diff for pull request {options.PullRequestId} from {options.ProjectKey}/{options.RepositorySlug}...");
            
            var diff = await client.GetPullRequestDiffAsync(options.ProjectKey, options.RepositorySlug, options.PullRequestId);
            
            Console.WriteLine();
            DisplayDiff(diff);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the get-pr-commits command.
    /// </summary>
    public static async Task<int> HandleGetPullRequestCommitsAsync(GetPullRequestCommitsOptions options)
    {
        try
        {
            using var client = new BitBucketClient();

            Console.WriteLine($"Fetching commits for pull request {options.PullRequestId} from {options.ProjectKey}/{options.RepositorySlug}...");
            
            var commits = await client.GetPullRequestCommitsAsync(options.ProjectKey, options.RepositorySlug, options.PullRequestId);
            
            Console.WriteLine();
            DisplayCommits(commits);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the get-pr-comments command.
    /// </summary>
    public static async Task<int> HandleGetPullRequestCommentsAsync(GetPullRequestCommentsOptions options)
    {
        try
        {
            using var client = new BitBucketClient();

            Console.WriteLine($"Fetching comments for pull request {options.PullRequestId} from {options.ProjectKey}/{options.RepositorySlug}...");
            
            var activities = await client.GetPullRequestActivitiesAsync(options.ProjectKey, options.RepositorySlug, options.PullRequestId);
            
            Console.WriteLine();
            DisplayComments(activities);

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the add-pr-comment command.
    /// </summary>
    public static async Task<int> HandleAddPullRequestCommentAsync(AddPullRequestCommentOptions options)
    {
        try
        {
            string text = ContentResolver.ResolveContent(options.Text, options.FilePath, "text");

            using var client = new BitBucketClient();

            Console.WriteLine($"Adding comment to pull request {options.PullRequestId} in {options.ProjectKey}/{options.RepositorySlug}...");
            
            var comment = await client.AddPullRequestCommentAsync(options.ProjectKey, options.RepositorySlug, options.PullRequestId, text);
            
            Console.WriteLine();
            Console.WriteLine("Comment added successfully!");
            Console.WriteLine($"  Comment ID: {comment.Id}");
            Console.WriteLine($"  Created:    {DateTimeOffset.FromUnixTimeMilliseconds(comment.CreatedDate).ToString("yyyy-MM-dd HH:mm:ss")}");

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Displays pull request information to the console.
    /// </summary>
    private static void DisplayPullRequest(BitBucketPullRequest pr)
    {
        Console.WriteLine("=== Pull Request Information ===");
        Console.WriteLine($"ID:          {pr.Id}");
        Console.WriteLine($"Title:       {pr.Title}");
        Console.WriteLine($"State:       {pr.State}");
        Console.WriteLine($"Open:        {pr.Open}");
        Console.WriteLine($"Author:      {pr.Author?.User?.DisplayName ?? "N/A"} ({pr.Author?.User?.Name ?? "N/A"})");
        Console.WriteLine($"Created:     {DateTimeOffset.FromUnixTimeMilliseconds(pr.CreatedDate).ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($"Updated:     {DateTimeOffset.FromUnixTimeMilliseconds(pr.UpdatedDate).ToString("yyyy-MM-dd HH:mm:ss")}");
        
        Console.WriteLine();
        Console.WriteLine("=== Source Branch ===");
        Console.WriteLine($"Branch:      {pr.FromRef?.DisplayId}");
        Console.WriteLine($"Repository:  {pr.FromRef?.Repository?.Slug}");
        Console.WriteLine($"Commit:      {pr.FromRef?.LatestCommit}");
        
        Console.WriteLine();
        Console.WriteLine("=== Target Branch ===");
        Console.WriteLine($"Branch:      {pr.ToRef?.DisplayId}");
        Console.WriteLine($"Repository:  {pr.ToRef?.Repository?.Slug}");
        Console.WriteLine($"Commit:      {pr.ToRef?.LatestCommit}");

        if (pr.Reviewers != null && pr.Reviewers.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Reviewers ({pr.Reviewers.Count}) ===");
            foreach (var reviewer in pr.Reviewers)
            {
                var approvalStatus = reviewer.Approved ? "✓ APPROVED" : "- Not Approved";
                Console.WriteLine($"  {reviewer.User?.DisplayName ?? "N/A"} ({reviewer.User?.Name ?? "N/A"}) - {approvalStatus}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== Description ===");
        if (!string.IsNullOrEmpty(pr.Description))
        {
            Console.WriteLine(pr.Description);
        }
        else
        {
            Console.WriteLine("(No description)");
        }

        if (pr.Links?.Self != null && pr.Links.Self.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"URL:         {pr.Links.Self[0].Href}");
        }
    }

    /// <summary>
    /// Displays diff information to the console.
    /// </summary>
    private static void DisplayDiff(BitBucketDiffResponse diff)
    {
        Console.WriteLine("=== Pull Request Diff ===");
        Console.WriteLine($"From:        {diff.FromHash}");
        Console.WriteLine($"To:          {diff.ToHash}");
        Console.WriteLine($"Truncated:   {diff.Truncated}");
        Console.WriteLine();

        if (diff.Diffs == null || diff.Diffs.Count == 0)
        {
            Console.WriteLine("No file changes found.");
            return;
        }

        Console.WriteLine($"Files changed: {diff.Diffs.Count}");
        Console.WriteLine();

        foreach (var fileDiff in diff.Diffs)
        {
            var sourcePath = fileDiff.Source?.ToString ?? "(none)";
            var destPath = fileDiff.Destination?.ToString ?? "(none)";
            
            if (sourcePath == destPath)
            {
                Console.WriteLine($"=== {destPath} ===");
            }
            else
            {
                Console.WriteLine($"=== {sourcePath} → {destPath} ===");
            }

            if (fileDiff.Hunks == null || fileDiff.Hunks.Count == 0)
            {
                Console.WriteLine("(Binary file or no changes)");
                Console.WriteLine();
                continue;
            }

            foreach (var hunk in fileDiff.Hunks)
            {
                Console.WriteLine($"@@ -{hunk.SourceLine},{hunk.SourceSpan} +{hunk.DestinationLine},{hunk.DestinationSpan} @@");
                
                if (hunk.Segments != null)
                {
                    foreach (var segment in hunk.Segments)
                    {
                        if (segment.Lines != null)
                        {
                            foreach (var line in segment.Lines)
                            {
                                var prefix = segment.Type switch
                                {
                                    "ADDED" => "+",
                                    "REMOVED" => "-",
                                    _ => " "
                                };
                                Console.WriteLine($"{prefix}{line.Line}");
                            }
                        }
                    }
                }
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Displays commits to the console.
    /// </summary>
    private static void DisplayCommits(List<BitBucketCommit> commits)
    {
        Console.WriteLine($"=== Pull Request Commits ({commits.Count}) ===");
        Console.WriteLine();

        if (commits.Count == 0)
        {
            Console.WriteLine("No commits found.");
            return;
        }

        foreach (var commit in commits)
        {
            Console.WriteLine($"Commit:      {commit.DisplayId}");
            Console.WriteLine($"Author:      {commit.Author?.Name ?? "N/A"} <{commit.Author?.EmailAddress ?? "N/A"}>");
            Console.WriteLine($"Date:        {DateTimeOffset.FromUnixTimeMilliseconds(commit.AuthorTimestamp).ToString("yyyy-MM-dd HH:mm:ss")}");
            
            if (!string.IsNullOrEmpty(commit.Message))
            {
                var lines = commit.Message.Split('\n');
                Console.WriteLine($"Message:     {lines[0]}");
                for (int i = 1; i < lines.Length; i++)
                {
                    Console.WriteLine($"             {lines[i]}");
                }
            }
            
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Displays comments and activities to the console.
    /// </summary>
    private static void DisplayComments(List<BitBucketActivity> activities)
    {
        var commentActivities = activities.Where(a => a.Comment != null).ToList();
        
        Console.WriteLine($"=== Pull Request Comments ({commentActivities.Count}) ===");
        Console.WriteLine();

        if (commentActivities.Count == 0)
        {
            Console.WriteLine("No comments found.");
            return;
        }

        foreach (var activity in commentActivities)
        {
            if (activity.Comment == null)
            {
                continue;
            }

            Console.WriteLine($"[{DateTimeOffset.FromUnixTimeMilliseconds(activity.CreatedDate).ToString("yyyy-MM-dd HH:mm:ss")}] {activity.Comment.Author?.DisplayName ?? "Unknown"}:");
            
            if (activity.CommentAnchor?.Path != null)
            {
                Console.WriteLine($"  File: {activity.CommentAnchor.Path} (Line {activity.CommentAnchor.Line})");
            }
            
            Console.WriteLine($"  {activity.Comment.Text}");
            
            if (activity.Comment.Comments != null && activity.Comment.Comments.Count > 0)
            {
                foreach (var reply in activity.Comment.Comments)
                {
                    Console.WriteLine($"    └─ [{DateTimeOffset.FromUnixTimeMilliseconds(reply.CreatedDate).ToString("yyyy-MM-dd HH:mm:ss")}] {reply.Author?.DisplayName ?? "Unknown"}:");
                    Console.WriteLine($"       {reply.Text}");
                }
            }
            
            Console.WriteLine();
        }
    }
}
