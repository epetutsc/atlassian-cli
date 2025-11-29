using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AtlassianCli.Models;

namespace AtlassianCli.Client;

/// <summary>
/// Client for interacting with Jira REST API.
/// Handles authentication, HTTP requests, and response parsing.
/// </summary>
public sealed class JiraClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new JiraClient instance using environment variables for configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are missing.</exception>
    public JiraClient()
    {
        _baseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL") 
            ?? throw new InvalidOperationException(
                "JIRA_BASE_URL environment variable is not set. " +
                "Please set it to your Jira instance URL (e.g., https://jira.example.com)");

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
        var apiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN");
        var username = Environment.GetEnvironmentVariable("JIRA_USERNAME");
        var password = Environment.GetEnvironmentVariable("JIRA_PASSWORD");

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
                "  - JIRA_USERNAME and JIRA_API_TOKEN, or\n" +
                "  - JIRA_USERNAME and JIRA_PASSWORD");
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Gets an issue by its key.
    /// </summary>
    /// <param name="issueKey">The key of the issue (e.g., PROJ-123).</param>
    /// <returns>The requested issue.</returns>
    public async Task<JiraIssue> GetIssueAsync(string issueKey)
    {
        var url = $"{_baseUrl}/rest/api/2/issue/{issueKey}?expand=renderedFields";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting issue {issueKey}");

        var result = await response.Content.ReadFromJsonAsync<JiraIssue>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize issue {issueKey}");
    }

    /// <summary>
    /// Creates a new issue.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="summary">The issue summary.</param>
    /// <param name="issueType">The issue type (e.g., Task, Bug, Story).</param>
    /// <param name="description">Optional description.</param>
    /// <returns>The created issue response.</returns>
    public async Task<CreateJiraIssueResponse> CreateIssueAsync(string projectKey, string summary, string issueType, string? description = null)
    {
        var request = new CreateJiraIssueRequest
        {
            Fields = new CreateJiraIssueFields
            {
                Project = new ProjectKey { Key = projectKey },
                Summary = summary,
                IssueType = new IssueTypeName { Name = issueType },
                Description = description
            }
        };

        var url = $"{_baseUrl}/rest/api/2/issue";
        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions);

        await EnsureSuccessAsync(response, "creating issue");

        var result = await response.Content.ReadFromJsonAsync<CreateJiraIssueResponse>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize created issue response");
    }

    /// <summary>
    /// Adds a comment to an issue.
    /// </summary>
    /// <param name="issueKey">The issue key.</param>
    /// <param name="body">The comment body.</param>
    /// <returns>The created comment.</returns>
    public async Task<JiraComment> AddCommentAsync(string issueKey, string body)
    {
        var request = new AddJiraCommentRequest { Body = body };

        var url = $"{_baseUrl}/rest/api/2/issue/{issueKey}/comment";
        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions);

        await EnsureSuccessAsync(response, $"adding comment to issue {issueKey}");

        var result = await response.Content.ReadFromJsonAsync<JiraComment>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize comment response");
    }

    /// <summary>
    /// Gets available transitions for an issue.
    /// </summary>
    /// <param name="issueKey">The issue key.</param>
    /// <returns>List of available transitions.</returns>
    public async Task<List<JiraTransition>> GetTransitionsAsync(string issueKey)
    {
        var url = $"{_baseUrl}/rest/api/2/issue/{issueKey}/transitions";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting transitions for issue {issueKey}");

        var result = await response.Content.ReadFromJsonAsync<JiraTransitionsResponse>(_jsonOptions);
        return result?.Transitions ?? new List<JiraTransition>();
    }

    /// <summary>
    /// Transitions an issue to a new status.
    /// </summary>
    /// <param name="issueKey">The issue key.</param>
    /// <param name="statusName">The target status name.</param>
    public async Task TransitionIssueAsync(string issueKey, string statusName)
    {
        // First, get available transitions
        var transitions = await GetTransitionsAsync(issueKey);
        
        var transition = transitions.FirstOrDefault(t => 
            t.Name.Equals(statusName, StringComparison.OrdinalIgnoreCase) ||
            (t.To?.Name?.Equals(statusName, StringComparison.OrdinalIgnoreCase) ?? false));

        if (transition == null)
        {
            var availableTransitions = string.Join(", ", transitions.Select(t => t.Name));
            throw new InvalidOperationException(
                $"Cannot transition to '{statusName}'. Available transitions: {availableTransitions}");
        }

        var request = new TransitionJiraIssueRequest
        {
            Transition = new TransitionId { Id = transition.Id }
        };

        var url = $"{_baseUrl}/rest/api/2/issue/{issueKey}/transitions";
        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions);

        await EnsureSuccessAsync(response, $"transitioning issue {issueKey} to {statusName}");
    }

    /// <summary>
    /// Searches for users by username or display name.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <returns>List of matching users.</returns>
    public async Task<List<JiraUserSearchResult>> SearchUsersAsync(string query)
    {
        var encodedQuery = Uri.EscapeDataString(query);
        var url = $"{_baseUrl}/rest/api/2/user/search?query={encodedQuery}";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"searching for user '{query}'");

        var result = await response.Content.ReadFromJsonAsync<List<JiraUserSearchResult>>(_jsonOptions);
        return result ?? new List<JiraUserSearchResult>();
    }

    /// <summary>
    /// Assigns a user to an issue.
    /// </summary>
    /// <param name="issueKey">The issue key.</param>
    /// <param name="username">The username or account ID to assign.</param>
    public async Task AssignIssueAsync(string issueKey, string username)
    {
        // First try to find the user
        var users = await SearchUsersAsync(username);
        
        AssignJiraIssueRequest request;
        if (users.Count > 0)
        {
            var user = users[0];
            request = new AssignJiraIssueRequest
            {
                AccountId = user.AccountId,
                Name = user.Name
            };
        }
        else
        {
            // If user not found, try assigning by name directly (for older Jira versions)
            request = new AssignJiraIssueRequest { Name = username };
        }

        var url = $"{_baseUrl}/rest/api/2/issue/{issueKey}/assignee";
        var response = await _httpClient.PutAsJsonAsync(url, request, _jsonOptions);

        await EnsureSuccessAsync(response, $"assigning user '{username}' to issue {issueKey}");
    }

    /// <summary>
    /// Updates an issue's description.
    /// </summary>
    /// <param name="issueKey">The issue key.</param>
    /// <param name="description">The new description.</param>
    public async Task UpdateIssueDescriptionAsync(string issueKey, string description)
    {
        var request = new UpdateJiraIssueRequest
        {
            Fields = new UpdateJiraIssueFields
            {
                Description = description
            }
        };

        var url = $"{_baseUrl}/rest/api/2/issue/{issueKey}";
        var response = await _httpClient.PutAsJsonAsync(url, request, _jsonOptions);

        await EnsureSuccessAsync(response, $"updating description of issue {issueKey}");
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

        // Try to parse Jira error response for more details
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
