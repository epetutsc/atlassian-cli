using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ConfluenceCli.Models;

namespace ConfluenceCli.Client;

/// <summary>
/// Client for interacting with Confluence REST API.
/// Handles authentication, HTTP requests, and response parsing.
/// </summary>
public class ConfluenceClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new ConfluenceClient instance using environment variables for configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are missing.</exception>
    public ConfluenceClient()
    {
        _baseUrl = Environment.GetEnvironmentVariable("CONFLUENCE_BASE_URL") 
            ?? throw new InvalidOperationException(
                "CONFLUENCE_BASE_URL environment variable is not set. " +
                "Please set it to your Confluence instance URL (e.g., https://confluence.example.com)");

        // Normalize base URL - remove trailing slash
        _baseUrl = _baseUrl.TrimEnd('/');

        _httpClient = new HttpClient();
        ConfigureAuthentication();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Configures authentication for the HTTP client.
    /// Supports both API token and username/password authentication.
    /// </summary>
    private void ConfigureAuthentication()
    {
        var apiToken = Environment.GetEnvironmentVariable("CONFLUENCE_API_TOKEN");
        var username = Environment.GetEnvironmentVariable("CONFLUENCE_USERNAME");
        var password = Environment.GetEnvironmentVariable("CONFLUENCE_PASSWORD");

        if (!string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(username))
        {
            // API Token authentication (username + token as password)
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
        else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            // Basic authentication (username + password)
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
        else
        {
            throw new InvalidOperationException(
                "Authentication not configured. Please set either:\n" +
                "  - CONFLUENCE_USERNAME and CONFLUENCE_API_TOKEN, or\n" +
                "  - CONFLUENCE_USERNAME and CONFLUENCE_PASSWORD");
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Creates a new page in the specified space.
    /// </summary>
    /// <param name="spaceKey">The key of the space where the page will be created.</param>
    /// <param name="title">The title of the new page.</param>
    /// <param name="body">The body content in storage format (XHTML).</param>
    /// <returns>The created page.</returns>
    public async Task<ConfluencePage> CreatePageAsync(string spaceKey, string title, string body)
    {
        var request = new CreatePageRequest
        {
            Title = title,
            Space = new SpaceReference { Key = spaceKey },
            Body = new PageBody
            {
                Storage = new StorageContent
                {
                    Value = body,
                    Representation = "storage"
                }
            }
        };

        var url = $"{_baseUrl}/rest/api/content";
        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions);

        await EnsureSuccessAsync(response, "creating page");

        var result = await response.Content.ReadFromJsonAsync<ConfluencePage>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize created page response");
    }

    /// <summary>
    /// Gets a page by its ID.
    /// </summary>
    /// <param name="pageId">The ID of the page to retrieve.</param>
    /// <param name="expand">Comma-separated list of properties to expand (default: body.storage,version,space).</param>
    /// <returns>The requested page.</returns>
    public async Task<ConfluencePage> GetPageByIdAsync(string pageId, string expand = "body.storage,body.view,version,space")
    {
        var url = $"{_baseUrl}/rest/api/content/{pageId}?expand={expand}";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting page with ID {pageId}");

        var result = await response.Content.ReadFromJsonAsync<ConfluencePage>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize page with ID {pageId}");
    }

    /// <summary>
    /// Gets a page by title and space key.
    /// </summary>
    /// <param name="spaceKey">The key of the space containing the page.</param>
    /// <param name="title">The title of the page to find.</param>
    /// <param name="expand">Comma-separated list of properties to expand.</param>
    /// <returns>The requested page, or null if not found.</returns>
    public async Task<ConfluencePage?> GetPageByTitleAsync(string spaceKey, string title, string expand = "body.storage,body.view,version,space")
    {
        var encodedTitle = Uri.EscapeDataString(title);
        var url = $"{_baseUrl}/rest/api/content?spaceKey={spaceKey}&title={encodedTitle}&expand={expand}";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"searching for page '{title}' in space '{spaceKey}'");

        var searchResults = await response.Content.ReadFromJsonAsync<SearchResults>(_jsonOptions);
        return searchResults?.Results.FirstOrDefault();
    }

    /// <summary>
    /// Updates an existing page by ID.
    /// </summary>
    /// <param name="pageId">The ID of the page to update.</param>
    /// <param name="newBody">The new body content in storage format.</param>
    /// <param name="appendContent">If true, appends to existing content instead of replacing.</param>
    /// <returns>The updated page.</returns>
    public async Task<ConfluencePage> UpdatePageAsync(string pageId, string newBody, bool appendContent = false)
    {
        // First, get the current page to retrieve the current version and title
        var currentPage = await GetPageByIdAsync(pageId, "body.storage,version");

        var finalBody = newBody;
        if (appendContent && currentPage.Body?.Storage != null)
        {
            finalBody = currentPage.Body.Storage.Value + newBody;
        }

        var request = new UpdatePageRequest
        {
            Id = pageId,
            Title = currentPage.Title,
            Body = new PageBody
            {
                Storage = new StorageContent
                {
                    Value = finalBody,
                    Representation = "storage"
                }
            },
            Version = new PageVersion
            {
                Number = currentPage.Version!.Number + 1
            }
        };

        var url = $"{_baseUrl}/rest/api/content/{pageId}";
        var response = await _httpClient.PutAsJsonAsync(url, request, _jsonOptions);

        await EnsureSuccessAsync(response, $"updating page with ID {pageId}");

        var result = await response.Content.ReadFromJsonAsync<ConfluencePage>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize updated page response");
    }

    /// <summary>
    /// Updates an existing page by title and space key.
    /// </summary>
    /// <param name="spaceKey">The key of the space containing the page.</param>
    /// <param name="title">The title of the page to update.</param>
    /// <param name="newBody">The new body content in storage format.</param>
    /// <param name="appendContent">If true, appends to existing content instead of replacing.</param>
    /// <returns>The updated page.</returns>
    public async Task<ConfluencePage> UpdatePageByTitleAsync(string spaceKey, string title, string newBody, bool appendContent = false)
    {
        var page = await GetPageByTitleAsync(spaceKey, title, "body.storage,version");
        if (page == null)
        {
            throw new InvalidOperationException($"Page with title '{title}' not found in space '{spaceKey}'");
        }

        return await UpdatePageAsync(page.Id, newBody, appendContent);
    }

    /// <summary>
    /// Ensures the HTTP response was successful, throwing a detailed exception if not.
    /// </summary>
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        var errorMessage = $"Error {operation}: HTTP {(int)response.StatusCode} ({response.ReasonPhrase})";

        // Try to parse Confluence error response for more details
        try
        {
            if (!string.IsNullOrEmpty(errorContent))
            {
                errorMessage += $"\nDetails: {errorContent}";
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        throw new HttpRequestException(errorMessage);
    }

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
