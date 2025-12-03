using AtlassianCli.Client;
using AtlassianCli.Models;

namespace AtlassianCli.Commands;

/// <summary>
/// Handles execution of CLI commands for Confluence operations.
/// </summary>
public static class ConfluenceCommandHandlers
{
    /// <summary>
    /// Handles the create-page command.
    /// </summary>
    public static async Task<int> HandleCreatePageAsync(CreatePageOptions options)
    {
        try
        {
            string body = ContentResolver.ResolveContent(options.Body, options.FilePath, "body");

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

            string body = ContentResolver.ResolveContent(options.Body, options.FilePath, "body");

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
}
