using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AtlassianCli.Models;

namespace AtlassianCli.Client;

/// <summary>
/// Client for interacting with BitBucket REST API.
/// Handles authentication, HTTP requests, and response parsing.
/// </summary>
public sealed class BitBucketClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new BitBucketClient instance using environment variables for configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are missing.</exception>
    public BitBucketClient()
    {
        _baseUrl = Environment.GetEnvironmentVariable("BITBUCKET_BASE_URL") 
            ?? throw new InvalidOperationException(
                "BITBUCKET_BASE_URL environment variable is not set. " +
                "Please set it to your BitBucket instance URL (e.g., https://bitbucket.example.com)");

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
    /// Supports Bearer token (PAT), API token with username, and username/password authentication.
    /// </summary>
    private void ConfigureAuthentication()
    {
        var apiToken = Environment.GetEnvironmentVariable("BITBUCKET_API_TOKEN");
        var username = Environment.GetEnvironmentVariable("BITBUCKET_USERNAME");
        var password = Environment.GetEnvironmentVariable("BITBUCKET_PASSWORD");

        if (!string.IsNullOrEmpty(apiToken) && string.IsNullOrEmpty(username))
        {
            // Bearer token authentication (Personal Access Token)
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        }
        else if (!string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(username))
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
                "  - BITBUCKET_API_TOKEN (for Personal Access Token / Bearer auth), or\n" +
                "  - BITBUCKET_USERNAME and BITBUCKET_API_TOKEN, or\n" +
                "  - BITBUCKET_USERNAME and BITBUCKET_PASSWORD");
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Gets a pull request by project key, repository slug, and pull request ID.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repositorySlug">The repository slug.</param>
    /// <param name="pullRequestId">The pull request ID.</param>
    /// <returns>The requested pull request.</returns>
    public async Task<BitBucketPullRequest> GetPullRequestAsync(string projectKey, string repositorySlug, int pullRequestId)
    {
        var url = $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}";
        var response = await HttpClientHelper.GetAsync(_httpClient, url);

        await EnsureSuccessAsync(response, $"getting pull request {pullRequestId}");

        var result = await response.Content.ReadFromJsonAsync<BitBucketPullRequest>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize pull request {pullRequestId}");
    }

    /// <summary>
    /// Gets the diff for a pull request.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repositorySlug">The repository slug.</param>
    /// <param name="pullRequestId">The pull request ID.</param>
    /// <returns>The pull request diff.</returns>
    public async Task<BitBucketDiffResponse> GetPullRequestDiffAsync(string projectKey, string repositorySlug, int pullRequestId)
    {
        var url = $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/diff";
        var response = await HttpClientHelper.GetAsync(_httpClient, url);

        await EnsureSuccessAsync(response, $"getting diff for pull request {pullRequestId}");

        var result = await response.Content.ReadFromJsonAsync<BitBucketDiffResponse>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize diff for pull request {pullRequestId}");
    }

    /// <summary>
    /// Gets commits for a pull request.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repositorySlug">The repository slug.</param>
    /// <param name="pullRequestId">The pull request ID.</param>
    /// <returns>List of commits in the pull request.</returns>
    public async Task<List<BitBucketCommit>> GetPullRequestCommitsAsync(string projectKey, string repositorySlug, int pullRequestId)
    {
        var commits = new List<BitBucketCommit>();
        var start = 0;
        var limit = 100;

        while (true)
        {
            var url = $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/commits?start={start}&limit={limit}";
            var response = await HttpClientHelper.GetAsync(_httpClient, url);

            await EnsureSuccessAsync(response, $"getting commits for pull request {pullRequestId}");

            var result = await response.Content.ReadFromJsonAsync<BitBucketCommitsResponse>(_jsonOptions);
            if (result?.Values != null)
            {
                commits.AddRange(result.Values);
            }

            if (result?.IsLastPage ?? true)
            {
                break;
            }

            if (result.NextPageStart.HasValue)
            {
                start = result.NextPageStart.Value;
            }
            else
            {
                break;
            }
        }

        return commits;
    }

    /// <summary>
    /// Gets activities (comments and events) for a pull request.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repositorySlug">The repository slug.</param>
    /// <param name="pullRequestId">The pull request ID.</param>
    /// <returns>List of activities in the pull request.</returns>
    public async Task<List<BitBucketActivity>> GetPullRequestActivitiesAsync(string projectKey, string repositorySlug, int pullRequestId)
    {
        var activities = new List<BitBucketActivity>();
        var start = 0;
        var limit = 100;

        while (true)
        {
            var url = $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/activities?start={start}&limit={limit}";
            var response = await HttpClientHelper.GetAsync(_httpClient, url);

            await EnsureSuccessAsync(response, $"getting activities for pull request {pullRequestId}");

            var result = await response.Content.ReadFromJsonAsync<BitBucketActivitiesResponse>(_jsonOptions);
            if (result?.Values != null)
            {
                activities.AddRange(result.Values);
            }

            if (result?.IsLastPage ?? true)
            {
                break;
            }

            if (result.NextPageStart.HasValue)
            {
                start = result.NextPageStart.Value;
            }
            else
            {
                break;
            }
        }

        return activities;
    }

    /// <summary>
    /// Adds a comment to a pull request.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repositorySlug">The repository slug.</param>
    /// <param name="pullRequestId">The pull request ID.</param>
    /// <param name="text">The comment text.</param>
    /// <returns>The created comment.</returns>
    public async Task<BitBucketComment> AddPullRequestCommentAsync(string projectKey, string repositorySlug, int pullRequestId, string text)
    {
        var request = new AddBitBucketCommentRequest { Text = text };

        var url = $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}/pull-requests/{pullRequestId}/comments";
        var response = await HttpClientHelper.PostAsJsonAsync(_httpClient, url, request, _jsonOptions);

        await EnsureSuccessAsync(response, $"adding comment to pull request {pullRequestId}");

        var result = await response.Content.ReadFromJsonAsync<BitBucketComment>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize comment response");
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

        // Try to include error details from response
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
