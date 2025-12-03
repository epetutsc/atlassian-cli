using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AtlassianCli.Models;

namespace AtlassianCli.Client;

/// <summary>
/// Client for interacting with Bamboo REST API.
/// Handles authentication, HTTP requests, and response parsing.
/// Supports read operations and triggering builds.
/// </summary>
public sealed class BambooClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new BambooClient instance using environment variables for configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are missing.</exception>
    public BambooClient()
    {
        _baseUrl = Environment.GetEnvironmentVariable("BAMBOO_BASE_URL") 
            ?? throw new InvalidOperationException(
                "BAMBOO_BASE_URL environment variable is not set. " +
                "Please set it to your Bamboo instance URL (e.g., https://bamboo.example.com)");

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
        var apiToken = Environment.GetEnvironmentVariable("BAMBOO_API_TOKEN");
        var username = Environment.GetEnvironmentVariable("BAMBOO_USERNAME");
        var password = Environment.GetEnvironmentVariable("BAMBOO_PASSWORD");

        if (!string.IsNullOrEmpty(apiToken) && string.IsNullOrEmpty(username))
        {
            // Bearer token authentication (Personal Access Token)
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        }
        else if (!string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(username))
        {
            // API Token authentication (username + token as password) for Atlassian Cloud
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
                "  - BAMBOO_API_TOKEN (for Personal Access Token / Bearer auth), or\n" +
                "  - BAMBOO_USERNAME and BAMBOO_API_TOKEN (for Atlassian Cloud), or\n" +
                "  - BAMBOO_USERNAME and BAMBOO_PASSWORD");
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // ==================== Project Operations ====================

    /// <summary>
    /// Gets all projects.
    /// </summary>
    /// <returns>List of all Bamboo projects.</returns>
    public async Task<List<BambooProject>> GetProjectsAsync()
    {
        var url = $"{_baseUrl}/rest/api/latest/project?expand=projects.project.plans&max-result=1000";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, "getting projects");

        var result = await response.Content.ReadFromJsonAsync<BambooProjectsResponse>(_jsonOptions);
        return result?.Projects.Project ?? new List<BambooProject>();
    }

    /// <summary>
    /// Gets a specific project by key.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <returns>The project details.</returns>
    public async Task<BambooProject> GetProjectAsync(string projectKey)
    {
        var url = $"{_baseUrl}/rest/api/latest/project/{projectKey}?expand=plans.plan";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting project {projectKey}");

        var result = await response.Content.ReadFromJsonAsync<BambooProject>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize project {projectKey}");
    }

    // ==================== Plan Operations ====================

    /// <summary>
    /// Gets all plans.
    /// </summary>
    /// <returns>List of all Bamboo plans.</returns>
    public async Task<List<BambooPlan>> GetPlansAsync()
    {
        var url = $"{_baseUrl}/rest/api/latest/plan?max-result=1000";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, "getting plans");

        var result = await response.Content.ReadFromJsonAsync<BambooPlansResponse>(_jsonOptions);
        return result?.Plans.Plan ?? new List<BambooPlan>();
    }

    /// <summary>
    /// Gets a specific plan by key.
    /// </summary>
    /// <param name="planKey">The plan key (e.g., PROJ-PLAN).</param>
    /// <returns>The plan details including configuration.</returns>
    public async Task<BambooPlan> GetPlanAsync(string planKey)
    {
        var url = $"{_baseUrl}/rest/api/latest/plan/{planKey}?expand=stages,branches,variableContext";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting plan {planKey}");

        var result = await response.Content.ReadFromJsonAsync<BambooPlan>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize plan {planKey}");
    }

    /// <summary>
    /// Gets branches for a plan.
    /// </summary>
    /// <param name="planKey">The plan key.</param>
    /// <returns>List of plan branches.</returns>
    public async Task<List<BambooBranch>> GetPlanBranchesAsync(string planKey)
    {
        var url = $"{_baseUrl}/rest/api/latest/plan/{planKey}/branch?max-result=1000";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting branches for plan {planKey}");

        var result = await response.Content.ReadFromJsonAsync<BambooBranchesList>(_jsonOptions);
        return result?.Branch ?? new List<BambooBranch>();
    }

    // ==================== Build Result Operations ====================

    /// <summary>
    /// Gets build results for a plan.
    /// </summary>
    /// <param name="planKey">The plan key.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>List of build results.</returns>
    public async Task<List<BambooBuildResult>> GetBuildResultsAsync(string planKey, int maxResults = 25)
    {
        var url = $"{_baseUrl}/rest/api/latest/result/{planKey}?expand=results.result&max-result={maxResults}";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting build results for plan {planKey}");

        var result = await response.Content.ReadFromJsonAsync<BambooBuildResultsResponse>(_jsonOptions);
        return result?.Results.Result ?? new List<BambooBuildResult>();
    }

    /// <summary>
    /// Gets a specific build result by key.
    /// </summary>
    /// <param name="buildResultKey">The build result key (e.g., PROJ-PLAN-123).</param>
    /// <returns>The build result details.</returns>
    public async Task<BambooBuildResult> GetBuildResultAsync(string buildResultKey)
    {
        var url = $"{_baseUrl}/rest/api/latest/result/{buildResultKey}?expand=stages.stage,changes.change";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting build result {buildResultKey}");

        var result = await response.Content.ReadFromJsonAsync<BambooBuildResult>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize build result {buildResultKey}");
    }

    /// <summary>
    /// Gets the latest build result for a plan.
    /// </summary>
    /// <param name="planKey">The plan key.</param>
    /// <returns>The latest build result.</returns>
    public async Task<BambooBuildResult> GetLatestBuildResultAsync(string planKey)
    {
        var url = $"{_baseUrl}/rest/api/latest/result/{planKey}/latest?expand=stages.stage,changes.change";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting latest build result for plan {planKey}");

        var result = await response.Content.ReadFromJsonAsync<BambooBuildResult>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize latest build result for {planKey}");
    }

    // ==================== Build Log Operations ====================

    /// <summary>
    /// Gets the build log for a specific build result.
    /// </summary>
    /// <param name="buildResultKey">The build result key (e.g., PROJ-PLAN-123).</param>
    /// <returns>The build log as a string.</returns>
    public async Task<string> GetBuildLogsAsync(string buildResultKey)
    {
        // Bamboo returns logs as plain text at this endpoint
        var url = $"{_baseUrl}/rest/api/latest/result/{buildResultKey}?expand=logEntries&max-results=10000";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting build logs for {buildResultKey}");

        // Try to get logEntries from JSON response first
        var result = await response.Content.ReadFromJsonAsync<BambooBuildResult>(_jsonOptions);
        
        if (result?.LogEntries?.LogEntry != null && result.LogEntries.LogEntry.Count > 0)
        {
            return string.Join(Environment.NewLine, result.LogEntries.LogEntry.Select(e => e.Log ?? string.Empty));
        }

        // Fallback: try to get raw log from the download endpoint
        return await GetBuildLogDownloadAsync(buildResultKey);
    }

    /// <summary>
    /// Downloads the raw build log file for a specific build result.
    /// </summary>
    /// <param name="buildResultKey">The build result key (e.g., PROJ-PLAN-123).</param>
    /// <returns>The build log as a string.</returns>
    public async Task<string> GetBuildLogDownloadAsync(string buildResultKey)
    {
        var url = $"{_baseUrl}/download/{buildResultKey}/build_logs/{buildResultKey}.log";
        var response = await _httpClient.GetAsync(url);

        // If download endpoint doesn't work, try the browse endpoint
        if (!response.IsSuccessStatusCode)
        {
            url = $"{_baseUrl}/browse/{buildResultKey}/log";
            response = await _httpClient.GetAsync(url);
        }

        await EnsureSuccessAsync(response, $"downloading build logs for {buildResultKey}");

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Gets the job log for a specific job within a build.
    /// </summary>
    /// <param name="buildResultKey">The build result key (e.g., PROJ-PLAN-123).</param>
    /// <param name="jobKey">The job key (e.g., PROJ-PLAN-JOB1).</param>
    /// <returns>The job log as a string.</returns>
    public async Task<string> GetJobLogsAsync(string buildResultKey, string jobKey)
    {
        // Job logs are available at a different endpoint
        var url = $"{_baseUrl}/download/{buildResultKey}/build_logs/{jobKey}.log";
        var response = await _httpClient.GetAsync(url);

        await EnsureSuccessAsync(response, $"getting job logs for {jobKey} in build {buildResultKey}");

        return await response.Content.ReadAsStringAsync();
    }

    // ==================== Queue Operations ====================

    /// <summary>
    /// Queues a build for a plan (triggers a new build).
    /// </summary>
    /// <param name="planKey">The plan key to build.</param>
    /// <returns>The queue response with build information.</returns>
    public async Task<BambooQueueResponse> QueueBuildAsync(string planKey)
    {
        var url = $"{_baseUrl}/rest/api/latest/queue/{planKey}";
        var response = await _httpClient.PostAsync(url, null);

        await EnsureSuccessAsync(response, $"queuing build for plan {planKey}");

        var result = await response.Content.ReadFromJsonAsync<BambooQueueResponse>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize queue response for {planKey}");
    }

    /// <summary>
    /// Queues a build for a specific branch.
    /// </summary>
    /// <param name="planKey">The plan key.</param>
    /// <param name="branchName">The branch name.</param>
    /// <returns>The queue response with build information.</returns>
    public async Task<BambooQueueResponse> QueueBranchBuildAsync(string planKey, string branchName)
    {
        var encodedBranch = Uri.EscapeDataString(branchName);
        var url = $"{_baseUrl}/rest/api/latest/queue/{planKey}/branch/{encodedBranch}";
        var response = await _httpClient.PostAsync(url, null);

        await EnsureSuccessAsync(response, $"queuing build for plan {planKey} branch {branchName}");

        var result = await response.Content.ReadFromJsonAsync<BambooQueueResponse>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize queue response for {planKey}/{branchName}");
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

        // Try to add more details from response
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
