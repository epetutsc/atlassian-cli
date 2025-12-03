using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AtlassianCli.Models;

namespace AtlassianCli.Tests;

/// <summary>
/// A mock HttpMessageHandler that simulates Bitbucket REST API responses.
/// </summary>
public class MockBitbucketHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public void SetupResponse(string path, HttpMethod method, Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var key = $"{method.Method}:{path}";
        _responses[key] = responseFactory;
    }

    public void SetupResponse(string path, HttpMethod method, object responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        SetupResponse(path, method, _ => new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(responseBody)
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        var key = $"{request.Method.Method}:{path}";

        // Try exact match first
        if (_responses.TryGetValue(key, out var handler))
        {
            return Task.FromResult(handler(request));
        }

        // Try path prefix matching for parameterized paths
        foreach (var kvp in _responses)
        {
            var registeredKey = kvp.Key;
            if (key.StartsWith(registeredKey.Split('?')[0]))
            {
                return Task.FromResult(kvp.Value(request));
            }
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"No mock configured for {request.Method} {path}")
        });
    }
}

/// <summary>
/// Integration tests for Bitbucket CLI commands using a mock HTTP handler.
/// </summary>
public class BitbucketIntegrationTests : IDisposable
{
    private readonly MockBitbucketHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly string _originalBaseUrl;
    private readonly string _originalUsername;
    private readonly string _originalApiToken;

    public BitbucketIntegrationTests()
    {
        // Save original environment variables
        _originalBaseUrl = Environment.GetEnvironmentVariable("BITBUCKET_BASE_URL") ?? string.Empty;
        _originalUsername = Environment.GetEnvironmentVariable("BITBUCKET_USERNAME") ?? string.Empty;
        _originalApiToken = Environment.GetEnvironmentVariable("BITBUCKET_API_TOKEN") ?? string.Empty;

        _mockHandler = new MockBitbucketHandler();
        _httpClient = new HttpClient(_mockHandler) { BaseAddress = new Uri("http://localhost") };

        // Set up environment variables for the test
        Environment.SetEnvironmentVariable("BITBUCKET_BASE_URL", "http://localhost");
        Environment.SetEnvironmentVariable("BITBUCKET_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("BITBUCKET_API_TOKEN", "test-api-token");
    }

    public void Dispose()
    {
        // Restore original environment variables
        if (string.IsNullOrEmpty(_originalBaseUrl))
            Environment.SetEnvironmentVariable("BITBUCKET_BASE_URL", null);
        else
            Environment.SetEnvironmentVariable("BITBUCKET_BASE_URL", _originalBaseUrl);

        if (string.IsNullOrEmpty(_originalUsername))
            Environment.SetEnvironmentVariable("BITBUCKET_USERNAME", null);
        else
            Environment.SetEnvironmentVariable("BITBUCKET_USERNAME", _originalUsername);

        if (string.IsNullOrEmpty(_originalApiToken))
            Environment.SetEnvironmentVariable("BITBUCKET_API_TOKEN", null);
        else
            Environment.SetEnvironmentVariable("BITBUCKET_API_TOKEN", _originalApiToken);

        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [Fact]
    public async Task GetRepository_ShouldReturnRepository()
    {
        // Arrange
        var expectedRepo = new BitbucketRepository
        {
            Slug = "my-repo",
            Name = "My Repository",
            Description = "Test repository",
            ScmId = "git",
            State = "AVAILABLE",
            Forkable = true,
            IsPublic = false,
            Project = new BitbucketProject { Key = "PROJ", Name = "Project" }
        };

        _mockHandler.SetupResponse("/rest/api/1.0/projects/PROJ/repos/my-repo", HttpMethod.Get, expectedRepo);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/1.0/projects/PROJ/repos/my-repo");
        var result = await response.Content.ReadFromJsonAsync<BitbucketRepository>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("my-repo", result.Slug);
        Assert.Equal("My Repository", result.Name);
        Assert.Equal("PROJ", result.Project?.Key);
    }

    [Fact]
    public async Task ListRepositories_ShouldReturnPagedResponse()
    {
        // Arrange
        var expectedResponse = new BitbucketPagedResponse<BitbucketRepository>
        {
            Values = new List<BitbucketRepository>
            {
                new BitbucketRepository { Slug = "repo-1", Name = "Repository 1" },
                new BitbucketRepository { Slug = "repo-2", Name = "Repository 2" }
            },
            Size = 2,
            Limit = 25,
            Start = 0,
            IsLastPage = true
        };

        _mockHandler.SetupResponse("/rest/api/1.0/projects/PROJ/repos", HttpMethod.Get, expectedResponse);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/1.0/projects/PROJ/repos?limit=25&start=0");
        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketRepository>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Values.Count);
        Assert.True(result.IsLastPage);
    }

    [Fact]
    public async Task ListBranches_ShouldReturnBranches()
    {
        // Arrange
        var expectedResponse = new BitbucketPagedResponse<BitbucketBranch>
        {
            Values = new List<BitbucketBranch>
            {
                new BitbucketBranch { Id = "refs/heads/main", DisplayId = "main", IsDefault = true },
                new BitbucketBranch { Id = "refs/heads/develop", DisplayId = "develop", IsDefault = false }
            },
            Size = 2,
            IsLastPage = true
        };

        _mockHandler.SetupResponse("/rest/api/1.0/projects/PROJ/repos/my-repo/branches", HttpMethod.Get, expectedResponse);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/1.0/projects/PROJ/repos/my-repo/branches?limit=25&start=0");
        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketBranch>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Values.Count);
        Assert.Equal("main", result.Values[0].DisplayId);
        Assert.True(result.Values[0].IsDefault);
    }

    [Fact]
    public async Task GetCommit_ShouldReturnCommit()
    {
        // Arrange
        var expectedCommit = new BitbucketCommit
        {
            Id = "abc123def456",
            DisplayId = "abc123d",
            Message = "Initial commit",
            Author = new BitbucketAuthor { Name = "John Doe", EmailAddress = "john@example.com" }
        };

        _mockHandler.SetupResponse("/rest/api/1.0/projects/PROJ/repos/my-repo/commits/abc123d", HttpMethod.Get, expectedCommit);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/1.0/projects/PROJ/repos/my-repo/commits/abc123d");
        var result = await response.Content.ReadFromJsonAsync<BitbucketCommit>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("abc123def456", result.Id);
        Assert.Equal("Initial commit", result.Message);
        Assert.Equal("John Doe", result.Author?.Name);
    }

    [Fact]
    public async Task ListPullRequests_ShouldReturnPullRequests()
    {
        // Arrange
        var expectedResponse = new BitbucketPagedResponse<BitbucketPullRequest>
        {
            Values = new List<BitbucketPullRequest>
            {
                new BitbucketPullRequest
                {
                    Id = 1,
                    Title = "Feature branch",
                    State = "OPEN",
                    FromRef = new BitbucketRef { DisplayId = "feature" },
                    ToRef = new BitbucketRef { DisplayId = "main" },
                    Author = new BitbucketPullRequestParticipant
                    {
                        User = new BitbucketUser { Name = "john", DisplayName = "John Doe" }
                    }
                }
            },
            Size = 1,
            IsLastPage = true
        };

        _mockHandler.SetupResponse("/rest/api/1.0/projects/PROJ/repos/my-repo/pull-requests", HttpMethod.Get, expectedResponse);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/1.0/projects/PROJ/repos/my-repo/pull-requests?state=OPEN&limit=25&start=0");
        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketPullRequest>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Single(result.Values);
        Assert.Equal("Feature branch", result.Values[0].Title);
        Assert.Equal("OPEN", result.Values[0].State);
    }

    [Fact]
    public async Task GetPullRequest_ShouldReturnPullRequestDetails()
    {
        // Arrange
        var expectedPr = new BitbucketPullRequest
        {
            Id = 42,
            Title = "Add new feature",
            Description = "This PR adds a new feature",
            State = "OPEN",
            FromRef = new BitbucketRef { DisplayId = "feature/new-feature" },
            ToRef = new BitbucketRef { DisplayId = "main" },
            Reviewers = new List<BitbucketPullRequestParticipant>
            {
                new BitbucketPullRequestParticipant
                {
                    User = new BitbucketUser { DisplayName = "Jane Doe" },
                    Approved = true
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/1.0/projects/PROJ/repos/my-repo/pull-requests/42", HttpMethod.Get, expectedPr);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/1.0/projects/PROJ/repos/my-repo/pull-requests/42");
        var result = await response.Content.ReadFromJsonAsync<BitbucketPullRequest>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Add new feature", result.Title);
        Assert.NotNull(result.Reviewers);
        Assert.Single(result.Reviewers);
        Assert.True(result.Reviewers[0].Approved);
    }

    [Fact]
    public async Task GetProject_ShouldReturnProject()
    {
        // Arrange
        var expectedProject = new BitbucketProject
        {
            Key = "PROJ",
            Name = "My Project",
            Description = "Project description",
            IsPublic = false,
            Type = "NORMAL"
        };

        _mockHandler.SetupResponse("/rest/api/1.0/projects/PROJ", HttpMethod.Get, expectedProject);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/1.0/projects/PROJ");
        var result = await response.Content.ReadFromJsonAsync<BitbucketProject>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("PROJ", result.Key);
        Assert.Equal("My Project", result.Name);
    }

    [Fact]
    public async Task GetBuildStatuses_ShouldReturnStatuses()
    {
        // Arrange
        var expectedResponse = new BitbucketPagedResponse<BitbucketBuildStatus>
        {
            Values = new List<BitbucketBuildStatus>
            {
                new BitbucketBuildStatus
                {
                    State = "SUCCESSFUL",
                    Key = "build-key",
                    Name = "CI Build",
                    Url = "http://ci.example.com/build/123"
                }
            },
            Size = 1,
            IsLastPage = true
        };

        _mockHandler.SetupResponse("/rest/build-status/1.0/commits/abc123", HttpMethod.Get, expectedResponse);

        // Act
        var response = await _httpClient.GetAsync("/rest/build-status/1.0/commits/abc123");
        var result = await response.Content.ReadFromJsonAsync<BitbucketPagedResponse<BitbucketBuildStatus>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Single(result.Values);
        Assert.Equal("SUCCESSFUL", result.Values[0].State);
    }

    [Fact]
    public async Task GetRepository_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/1.0/projects/INVALID/repos/nonexistent", HttpMethod.Get, _ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"errors\":[{\"message\":\"Repository not found\"}]}")
            });

        // Act
        var response = await _httpClient.GetAsync("/rest/api/1.0/projects/INVALID/repos/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void BitbucketModels_ShouldSerializeCorrectly()
    {
        // Arrange
        var repo = new BitbucketRepository
        {
            Slug = "my-repo",
            Name = "My Repository",
            Description = "Test repository",
            ScmId = "git",
            State = "AVAILABLE",
            Forkable = true,
            IsPublic = false,
            Project = new BitbucketProject { Key = "PROJ", Name = "Project" },
            Links = new BitbucketLinks
            {
                Clone = new List<BitbucketCloneLink>
                {
                    new BitbucketCloneLink { Href = "https://git.example.com/proj/my-repo.git", Name = "https" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(repo);
        var deserialized = JsonSerializer.Deserialize<BitbucketRepository>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(repo.Slug, deserialized.Slug);
        Assert.Equal(repo.Name, deserialized.Name);
        Assert.Equal(repo.Project?.Key, deserialized.Project?.Key);
    }

    [Fact]
    public void BitbucketPullRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var pr = new BitbucketPullRequest
        {
            Id = 123,
            Title = "Feature PR",
            Description = "Adding new feature",
            State = "OPEN",
            Open = true,
            Closed = false,
            FromRef = new BitbucketRef
            {
                Id = "refs/heads/feature",
                DisplayId = "feature",
                LatestCommit = "abc123"
            },
            ToRef = new BitbucketRef
            {
                Id = "refs/heads/main",
                DisplayId = "main"
            },
            Author = new BitbucketPullRequestParticipant
            {
                User = new BitbucketUser { Name = "john", DisplayName = "John Doe" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(pr);
        var deserialized = JsonSerializer.Deserialize<BitbucketPullRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(pr.Id, deserialized.Id);
        Assert.Equal(pr.Title, deserialized.Title);
        Assert.Equal(pr.FromRef?.DisplayId, deserialized.FromRef?.DisplayId);
        Assert.Equal(pr.Author?.User?.DisplayName, deserialized.Author?.User?.DisplayName);
    }

    [Fact]
    public void BitbucketPipeline_ShouldSerializeCorrectly()
    {
        // Arrange
        var pipeline = new BitbucketPipeline
        {
            Uuid = "{uuid-123}",
            BuildNumber = 42,
            State = new BitbucketPipelineState
            {
                Name = "COMPLETED",
                Type = "pipeline_state_completed",
                Result = new BitbucketPipelineResult { Name = "SUCCESSFUL" }
            },
            Target = new BitbucketPipelineTargetInfo
            {
                Type = "pipeline_ref_target",
                RefType = "branch",
                RefName = "main"
            },
            DurationInSeconds = 120
        };

        // Act
        var json = JsonSerializer.Serialize(pipeline);
        var deserialized = JsonSerializer.Deserialize<BitbucketPipeline>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(pipeline.Uuid, deserialized.Uuid);
        Assert.Equal(pipeline.BuildNumber, deserialized.BuildNumber);
        Assert.Equal(pipeline.State?.Name, deserialized.State?.Name);
        Assert.Equal(pipeline.Target?.RefName, deserialized.Target?.RefName);
    }

    [Fact]
    public void BitbucketPipelineTriggerRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new BitbucketPipelineTriggerRequest
        {
            Target = new BitbucketPipelineTarget
            {
                Type = "pipeline_ref_target",
                RefType = "branch",
                RefName = "main",
                Selector = new BitbucketPipelineSelector
                {
                    Type = "custom",
                    Pattern = "deploy-to-prod"
                }
            },
            Variables = new List<BitbucketPipelineVariable>
            {
                new BitbucketPipelineVariable { Key = "DEPLOY_ENV", Value = "production" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<BitbucketPipelineTriggerRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(request.Target.RefName, deserialized.Target.RefName);
        Assert.Equal(request.Target.Selector?.Pattern, deserialized.Target.Selector?.Pattern);
        Assert.NotNull(deserialized.Variables);
        Assert.Single(deserialized.Variables);
        Assert.Equal("DEPLOY_ENV", deserialized.Variables[0].Key);
    }

    [Fact]
    public void BitbucketPagedResponse_ShouldHandlePagination()
    {
        // Arrange
        var response = new BitbucketPagedResponse<BitbucketRepository>
        {
            Values = new List<BitbucketRepository>
            {
                new BitbucketRepository { Slug = "repo-1" }
            },
            Size = 1,
            Limit = 25,
            Start = 0,
            IsLastPage = false,
            NextPageStart = 25
        };

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<BitbucketPagedResponse<BitbucketRepository>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Single(deserialized.Values);
        Assert.False(deserialized.IsLastPage);
        Assert.Equal(25, deserialized.NextPageStart);
    }

    [Fact]
    public void BitbucketBranchRestriction_ShouldSerializeCorrectly()
    {
        // Arrange
        var restriction = new BitbucketBranchRestriction
        {
            Id = 1,
            Type = "no-deletes",
            Matcher = new BitbucketBranchMatcher
            {
                Id = "main",
                DisplayId = "main",
                Type = new BitbucketMatcherType { Id = "BRANCH", Name = "Branch" },
                Active = true
            },
            Users = new List<BitbucketUser>
            {
                new BitbucketUser { Name = "admin", DisplayName = "Admin User" }
            },
            Groups = new List<string> { "developers" }
        };

        // Act
        var json = JsonSerializer.Serialize(restriction);
        var deserialized = JsonSerializer.Deserialize<BitbucketBranchRestriction>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("no-deletes", deserialized.Type);
        Assert.Equal("main", deserialized.Matcher?.DisplayId);
        Assert.NotNull(deserialized.Users);
        Assert.Single(deserialized.Users);
        Assert.NotNull(deserialized.Groups);
        Assert.Contains("developers", deserialized.Groups);
    }

    [Fact]
    public void BitbucketWebhook_ShouldSerializeCorrectly()
    {
        // Arrange
        var webhook = new BitbucketWebhook
        {
            Id = 1,
            Name = "Build Trigger",
            Url = "https://ci.example.com/webhook",
            Events = new List<string> { "repo:refs_changed", "pr:merged" },
            Active = true
        };

        // Act
        var json = JsonSerializer.Serialize(webhook);
        var deserialized = JsonSerializer.Deserialize<BitbucketWebhook>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("Build Trigger", deserialized.Name);
        Assert.True(deserialized.Active);
        Assert.Equal(2, deserialized.Events.Count);
    }
}
