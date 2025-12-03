using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AtlassianCli.Models;

namespace AtlassianCli.Client;

/// <summary>
/// Client for interacting with Bitbucket REST API.
/// Handles authentication, HTTP requests, and response parsing.
/// Supports both Bitbucket Server (on-premises) and Bitbucket Cloud.
/// </summary>
public sealed class BitbucketClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly bool _isCloud;

    /// <summary>
    /// Creates a new BitbucketClient instance using environment variables for configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are missing.</exception>
    public BitbucketClient()
    {
        _baseUrl = Environment.GetEnvironmentVariable("BITBUCKET_BASE_URL") 
            ?? throw new InvalidOperationException(
                "BITBUCKET_BASE_URL environment variable is not set. " +
                "Please set it to your Bitbucket instance URL (e.g., https://bitbucket.example.com for Server or https://api.bitbucket.org for Cloud)");

        // Normalize base URL - remove trailing slash
        _baseUrl = _baseUrl.TrimEnd('/');

        // Detect if this is Bitbucket Cloud
        _isCloud = _baseUrl.Contains("api.bitbucket.org", StringComparison.OrdinalIgnoreCase) ||
                   _baseUrl.Contains("bitbucket.org", StringComparison.OrdinalIgnoreCase);

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
            // API Token authentication (username + token as password) for Bitbucket Cloud
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
                "  - BITBUCKET_USERNAME and BITBUCKET_API_TOKEN (for Bitbucket Cloud), or\n" +
                "  - BITBUCKET_USERNAME and BITBUCKET_PASSWORD");
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // ==================== Repository Operations ====================

    /// <summary>
    /// Gets a repository by project key and repository slug.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <returns>The requested repository.</returns>
    public async Task<BitbucketRepository> GetRepositoryAsync(string projectKey, string repoSlug)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}"
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repoSlug}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting repository {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketRepository>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize repository {projectKey}/{repoSlug}");
    }

    /// <summary>
    /// Lists repositories in a project.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="start">Start index for pagination.</param>
    /// <returns>Paginated list of repositories.</returns>
    public async Task<BitbucketPagedResponse<BitbucketRepository>> ListRepositoriesAsync(string projectKey, int limit = 25, int start = 0)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}?pagelen={limit}&page={start / limit + 1}"
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos?limit={limit}&start={start}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"listing repositories in project {projectKey}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketRepository>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketRepository>();
    }

    // ==================== Branch Operations ====================

    /// <summary>
    /// Lists branches in a repository.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="start">Start index for pagination.</param>
    /// <returns>Paginated list of branches.</returns>
    public async Task<BitbucketPagedResponse<BitbucketBranch>> ListBranchesAsync(string projectKey, string repoSlug, int limit = 25, int start = 0)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/refs/branches?pagelen={limit}&page={start / limit + 1}"
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repoSlug}/branches?limit={limit}&start={start}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"listing branches in {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketBranch>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketBranch>();
    }

    /// <summary>
    /// Gets the default branch of a repository.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <returns>The default branch.</returns>
    public async Task<BitbucketBranch> GetDefaultBranchAsync(string projectKey, string repoSlug)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/refs/branches?q=name=\"main\" OR name=\"master\""
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repoSlug}/default-branch";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting default branch for {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketBranch>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to get default branch for {projectKey}/{repoSlug}");
    }

    // ==================== Commit Operations ====================

    /// <summary>
    /// Lists commits in a repository.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="branch">Optional branch name to filter commits.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="start">Start index for pagination.</param>
    /// <returns>Paginated list of commits.</returns>
    public async Task<BitbucketPagedResponse<BitbucketCommit>> ListCommitsAsync(string projectKey, string repoSlug, string? branch = null, int limit = 25, int start = 0)
    {
        string url;
        if (_isCloud)
        {
            url = $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/commits?pagelen={limit}&page={start / limit + 1}";
            if (!string.IsNullOrEmpty(branch))
            {
                url += $"&include={Uri.EscapeDataString(branch)}";
            }
        }
        else
        {
            url = $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repoSlug}/commits?limit={limit}&start={start}";
            if (!string.IsNullOrEmpty(branch))
            {
                url += $"&until={Uri.EscapeDataString(branch)}";
            }
        }

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"listing commits in {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketCommit>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketCommit>();
    }

    /// <summary>
    /// Gets a specific commit by its ID.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="commitId">The commit ID (hash).</param>
    /// <returns>The requested commit.</returns>
    public async Task<BitbucketCommit> GetCommitAsync(string projectKey, string repoSlug, string commitId)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/commit/{commitId}"
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repoSlug}/commits/{commitId}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting commit {commitId} in {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketCommit>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize commit {commitId}");
    }

    // ==================== Pull Request Operations ====================

    /// <summary>
    /// Lists pull requests in a repository.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="state">Filter by state (OPEN, MERGED, DECLINED, ALL).</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="start">Start index for pagination.</param>
    /// <returns>Paginated list of pull requests.</returns>
    public async Task<BitbucketPagedResponse<BitbucketPullRequest>> ListPullRequestsAsync(string projectKey, string repoSlug, string state = "OPEN", int limit = 25, int start = 0)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/pullrequests?state={state.ToUpperInvariant()}&pagelen={limit}&page={start / limit + 1}"
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repoSlug}/pull-requests?state={state.ToUpperInvariant()}&limit={limit}&start={start}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"listing pull requests in {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketPullRequest>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketPullRequest>();
    }

    /// <summary>
    /// Gets a specific pull request by ID.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="pullRequestId">The pull request ID.</param>
    /// <returns>The requested pull request.</returns>
    public async Task<BitbucketPullRequest> GetPullRequestAsync(string projectKey, string repoSlug, int pullRequestId)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/pullrequests/{pullRequestId}"
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repoSlug}/pull-requests/{pullRequestId}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting pull request {pullRequestId} in {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPullRequest>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize pull request {pullRequestId}");
    }

    // ==================== Build Status Operations ====================

    /// <summary>
    /// Gets build statuses for a commit.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="commitId">The commit ID.</param>
    /// <returns>List of build statuses.</returns>
    public async Task<BitbucketPagedResponse<BitbucketBuildStatus>> GetBuildStatusesAsync(string projectKey, string repoSlug, string commitId)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/commit/{commitId}/statuses"
            : $"{_baseUrl}/rest/build-status/1.0/commits/{commitId}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting build statuses for commit {commitId}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketBuildStatus>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketBuildStatus>();
    }

    // ==================== Configuration/Settings Operations ====================

    /// <summary>
    /// Gets a project by its key.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <returns>The project details.</returns>
    public async Task<BitbucketProject> GetProjectAsync(string projectKey)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/workspaces/{projectKey}"
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting project {projectKey}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketProject>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to deserialize project {projectKey}");
    }

    /// <summary>
    /// Lists projects.
    /// </summary>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="start">Start index for pagination.</param>
    /// <returns>Paginated list of projects.</returns>
    public async Task<BitbucketPagedResponse<BitbucketProject>> ListProjectsAsync(int limit = 25, int start = 0)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/workspaces?pagelen={limit}&page={start / limit + 1}"
            : $"{_baseUrl}/rest/api/1.0/projects?limit={limit}&start={start}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, "listing projects");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketProject>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketProject>();
    }

    /// <summary>
    /// Gets branch restrictions for a repository.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <returns>List of branch restrictions.</returns>
    public async Task<BitbucketPagedResponse<BitbucketBranchRestriction>> GetBranchRestrictionsAsync(string projectKey, string repoSlug)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/branch-restrictions"
            : $"{_baseUrl}/rest/branch-permissions/2.0/projects/{projectKey}/repos/{repoSlug}/restrictions";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting branch restrictions for {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketBranchRestriction>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketBranchRestriction>();
    }

    /// <summary>
    /// Gets webhooks configured for a repository.
    /// </summary>
    /// <param name="projectKey">The project key.</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <returns>List of webhooks.</returns>
    public async Task<BitbucketPagedResponse<BitbucketWebhook>> GetWebhooksAsync(string projectKey, string repoSlug)
    {
        var url = _isCloud
            ? $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/hooks"
            : $"{_baseUrl}/rest/api/1.0/projects/{projectKey}/repos/{repoSlug}/webhooks";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting webhooks for {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketWebhook>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketWebhook>();
    }

    // ==================== Pipeline Operations ====================

    /// <summary>
    /// Gets pipeline configuration for a repository.
    /// </summary>
    /// <param name="projectKey">The project key (workspace for Cloud).</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <returns>Pipeline configuration.</returns>
    public async Task<BitbucketPipelineConfiguration> GetPipelineConfigurationAsync(string projectKey, string repoSlug)
    {
        if (!_isCloud)
        {
            throw new InvalidOperationException("Pipeline configuration is only available for Bitbucket Cloud. For Bitbucket Server, use build status APIs or external CI/CD tools.");
        }

        var url = $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/pipelines_config";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting pipeline configuration for {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPipelineConfiguration>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to get pipeline configuration for {projectKey}/{repoSlug}");
    }

    /// <summary>
    /// Lists pipelines (runs) for a repository.
    /// </summary>
    /// <param name="projectKey">The project key (workspace for Cloud).</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <returns>Paginated list of pipeline runs.</returns>
    public async Task<BitbucketPagedResponse<BitbucketPipeline>> ListPipelinesAsync(string projectKey, string repoSlug, int limit = 25)
    {
        if (!_isCloud)
        {
            throw new InvalidOperationException("Pipelines are only available for Bitbucket Cloud. For Bitbucket Server, use build status APIs or external CI/CD tools.");
        }

        var url = $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/pipelines/?pagelen={limit}&sort=-created_on";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"listing pipelines for {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketPipeline>>(_jsonOptions);
        return result ?? new BitbucketPagedResponse<BitbucketPipeline>();
    }

    /// <summary>
    /// Gets a specific pipeline by UUID.
    /// </summary>
    /// <param name="projectKey">The project key (workspace for Cloud).</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="pipelineUuid">The pipeline UUID.</param>
    /// <returns>The pipeline details.</returns>
    public async Task<BitbucketPipeline> GetPipelineAsync(string projectKey, string repoSlug, string pipelineUuid)
    {
        if (!_isCloud)
        {
            throw new InvalidOperationException("Pipelines are only available for Bitbucket Cloud. For Bitbucket Server, use build status APIs or external CI/CD tools.");
        }

        var url = $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/pipelines/{pipelineUuid}";

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response, $"getting pipeline {pipelineUuid} for {projectKey}/{repoSlug}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPipeline>(_jsonOptions);
        return result ?? throw new InvalidOperationException($"Failed to get pipeline {pipelineUuid}");
    }

    /// <summary>
    /// Triggers a pipeline run on a branch.
    /// </summary>
    /// <param name="projectKey">The project key (workspace for Cloud).</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="branch">The branch name to run the pipeline on.</param>
    /// <param name="customPipeline">Optional custom pipeline name to run.</param>
    /// <param name="variables">Optional pipeline variables.</param>
    /// <returns>The triggered pipeline.</returns>
    public async Task<BitbucketPipeline> TriggerPipelineAsync(string projectKey, string repoSlug, string branch, string? customPipeline = null, Dictionary<string, string>? variables = null)
    {
        if (!_isCloud)
        {
            throw new InvalidOperationException("Pipelines are only available for Bitbucket Cloud. For Bitbucket Server, use external CI/CD tools like Jenkins, Bamboo, or GitHub Actions.");
        }

        var request = new BitbucketPipelineTriggerRequest
        {
            Target = new BitbucketPipelineTarget
            {
                Type = "pipeline_ref_target",
                RefType = "branch",
                RefName = branch
            }
        };

        if (!string.IsNullOrEmpty(customPipeline))
        {
            request.Target.Selector = new BitbucketPipelineSelector
            {
                Type = "custom",
                Pattern = customPipeline
            };
        }

        if (variables != null && variables.Count > 0)
        {
            request.Variables = variables.Select(kvp => new BitbucketPipelineVariable
            {
                Key = kvp.Key,
                Value = kvp.Value,
                Secured = false
            }).ToList();
        }

        var url = $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/pipelines/";

        var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions);
        await EnsureSuccessAsync(response, $"triggering pipeline for {projectKey}/{repoSlug} on branch {branch}");

        var result = await response.Content.ReadFromJsonAsync<BitbucketPipeline>(_jsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize pipeline response");
    }

    /// <summary>
    /// Stops a running pipeline.
    /// </summary>
    /// <param name="projectKey">The project key (workspace for Cloud).</param>
    /// <param name="repoSlug">The repository slug.</param>
    /// <param name="pipelineUuid">The pipeline UUID to stop.</param>
    public async Task StopPipelineAsync(string projectKey, string repoSlug, string pipelineUuid)
    {
        if (!_isCloud)
        {
            throw new InvalidOperationException("Pipelines are only available for Bitbucket Cloud.");
        }

        var url = $"{_baseUrl}/2.0/repositories/{projectKey}/{repoSlug}/pipelines/{pipelineUuid}/stopPipeline";

        var response = await _httpClient.PostAsync(url, null);
        await EnsureSuccessAsync(response, $"stopping pipeline {pipelineUuid} for {projectKey}/{repoSlug}");
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

        // Try to parse Bitbucket error response for more details
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
