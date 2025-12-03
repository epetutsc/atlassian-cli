using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AtlassianCli.Models;

namespace AtlassianCli.Tests;

/// <summary>
/// A mock HttpMessageHandler that simulates Bamboo REST API responses.
/// </summary>
public class MockBambooHandler : HttpMessageHandler
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
/// Integration tests for Bamboo CLI commands using a mock HTTP handler.
/// </summary>
public class BambooIntegrationTests : IDisposable
{
    private readonly MockBambooHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly string _originalBaseUrl;
    private readonly string _originalUsername;
    private readonly string _originalApiToken;

    public BambooIntegrationTests()
    {
        // Save original environment variables
        _originalBaseUrl = Environment.GetEnvironmentVariable("BAMBOO_BASE_URL") ?? string.Empty;
        _originalUsername = Environment.GetEnvironmentVariable("BAMBOO_USERNAME") ?? string.Empty;
        _originalApiToken = Environment.GetEnvironmentVariable("BAMBOO_API_TOKEN") ?? string.Empty;

        _mockHandler = new MockBambooHandler();
        _httpClient = new HttpClient(_mockHandler) { BaseAddress = new Uri("http://localhost") };

        // Set up environment variables for the test
        Environment.SetEnvironmentVariable("BAMBOO_BASE_URL", "http://localhost");
        Environment.SetEnvironmentVariable("BAMBOO_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("BAMBOO_API_TOKEN", "test-api-token");
    }

    public void Dispose()
    {
        // Restore original environment variables
        if (string.IsNullOrEmpty(_originalBaseUrl))
            Environment.SetEnvironmentVariable("BAMBOO_BASE_URL", null);
        else
            Environment.SetEnvironmentVariable("BAMBOO_BASE_URL", _originalBaseUrl);

        if (string.IsNullOrEmpty(_originalUsername))
            Environment.SetEnvironmentVariable("BAMBOO_USERNAME", null);
        else
            Environment.SetEnvironmentVariable("BAMBOO_USERNAME", _originalUsername);

        if (string.IsNullOrEmpty(_originalApiToken))
            Environment.SetEnvironmentVariable("BAMBOO_API_TOKEN", null);
        else
            Environment.SetEnvironmentVariable("BAMBOO_API_TOKEN", _originalApiToken);

        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [Fact]
    public async Task GetProjects_ShouldReturnProjects()
    {
        // Arrange
        var projectsResponse = new BambooProjectsResponse
        {
            Projects = new BambooProjectsList
            {
                Size = 2,
                StartIndex = 0,
                MaxResult = 25,
                Project = new List<BambooProject>
                {
                    new BambooProject
                    {
                        Key = "PROJ",
                        Name = "Test Project",
                        Description = "A test project",
                        Link = new BambooLink { Href = "http://localhost/browse/PROJ" }
                    },
                    new BambooProject
                    {
                        Key = "DEMO",
                        Name = "Demo Project",
                        Description = "A demo project"
                    }
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/latest/project", HttpMethod.Get, projectsResponse);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/latest/project");
        var result = await response.Content.ReadFromJsonAsync<BambooProjectsResponse>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Projects.Project.Count);
        Assert.Equal("PROJ", result.Projects.Project[0].Key);
        Assert.Equal("Test Project", result.Projects.Project[0].Name);
    }

    [Fact]
    public async Task GetProject_ShouldReturnProject()
    {
        // Arrange
        var project = new BambooProject
        {
            Key = "PROJ",
            Name = "Test Project",
            Description = "A test project",
            Link = new BambooLink { Href = "http://localhost/browse/PROJ" },
            Plans = new BambooPlansList
            {
                Size = 2,
                Plan = new List<BambooPlan>
                {
                    new BambooPlan { Key = "PROJ-BUILD", Name = "Build Plan", Enabled = true },
                    new BambooPlan { Key = "PROJ-DEPLOY", Name = "Deploy Plan", Enabled = true }
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/latest/project/PROJ", HttpMethod.Get, project);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/latest/project/PROJ");
        var result = await response.Content.ReadFromJsonAsync<BambooProject>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("PROJ", result.Key);
        Assert.Equal("Test Project", result.Name);
        Assert.Equal(2, result.Plans?.Plan.Count);
    }

    [Fact]
    public async Task GetPlans_ShouldReturnPlans()
    {
        // Arrange
        var plansResponse = new BambooPlansResponse
        {
            Plans = new BambooPlansList
            {
                Size = 2,
                StartIndex = 0,
                MaxResult = 25,
                Plan = new List<BambooPlan>
                {
                    new BambooPlan
                    {
                        Key = "PROJ-BUILD",
                        Name = "Build Plan",
                        ShortName = "BUILD",
                        ProjectKey = "PROJ",
                        ProjectName = "Test Project",
                        Enabled = true,
                        IsBuilding = false,
                        IsActive = true
                    },
                    new BambooPlan
                    {
                        Key = "PROJ-DEPLOY",
                        Name = "Deploy Plan",
                        ShortName = "DEPLOY",
                        ProjectKey = "PROJ",
                        ProjectName = "Test Project",
                        Enabled = true,
                        IsBuilding = true,
                        IsActive = true
                    }
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/latest/plan", HttpMethod.Get, plansResponse);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/latest/plan");
        var result = await response.Content.ReadFromJsonAsync<BambooPlansResponse>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Plans.Plan.Count);
        Assert.Equal("PROJ-BUILD", result.Plans.Plan[0].Key);
        Assert.True(result.Plans.Plan[1].IsBuilding);
    }

    [Fact]
    public async Task GetPlan_ShouldReturnPlanWithConfiguration()
    {
        // Arrange
        var plan = new BambooPlan
        {
            Key = "PROJ-BUILD",
            Name = "Build Plan",
            ShortName = "BUILD",
            Description = "Main build plan",
            ProjectKey = "PROJ",
            ProjectName = "Test Project",
            Enabled = true,
            Type = "chain",
            AverageBuildTimeInSeconds = 120.5,
            IsBuilding = false,
            IsActive = true,
            IsFavourite = true,
            Link = new BambooLink { Href = "http://localhost/browse/PROJ-BUILD" },
            Stages = new BambooStagesList
            {
                Size = 2,
                Stage = new List<BambooStage>
                {
                    new BambooStage { Name = "Build Stage", Description = "Compile and test" },
                    new BambooStage { Name = "Package Stage", Description = "Create artifacts" }
                }
            },
            VariableContext = new BambooVariableContext
            {
                Size = 2,
                Variable = new List<BambooVariable>
                {
                    new BambooVariable { Name = "BUILD_ENV", Value = "production" },
                    new BambooVariable { Name = "DEBUG", Value = "false" }
                }
            },
            Branches = new BambooBranchesList
            {
                Size = 1,
                Branch = new List<BambooBranch>
                {
                    new BambooBranch { Key = "PROJ-BUILD0", Name = "main", Enabled = true }
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/latest/plan/PROJ-BUILD", HttpMethod.Get, plan);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/latest/plan/PROJ-BUILD");
        var result = await response.Content.ReadFromJsonAsync<BambooPlan>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("PROJ-BUILD", result.Key);
        Assert.Equal("Build Plan", result.Name);
        Assert.Equal("chain", result.Type);
        Assert.Equal(120.5, result.AverageBuildTimeInSeconds);
        Assert.Equal(2, result.Stages?.Stage.Count);
        Assert.Equal(2, result.VariableContext?.Variable.Count);
        Assert.Equal("BUILD_ENV", result.VariableContext?.Variable[0].Name);
    }

    [Fact]
    public async Task GetPlanBranches_ShouldReturnBranches()
    {
        // Arrange
        var branchesList = new BambooBranchesList
        {
            Size = 3,
            StartIndex = 0,
            MaxResult = 25,
            Branch = new List<BambooBranch>
            {
                new BambooBranch { Key = "PROJ-BUILD0", Name = "main", ShortName = "main", Enabled = true },
                new BambooBranch { Key = "PROJ-BUILD1", Name = "develop", ShortName = "develop", Enabled = true },
                new BambooBranch { Key = "PROJ-BUILD2", Name = "feature/new-feature", ShortName = "new-feature", Enabled = false }
            }
        };

        _mockHandler.SetupResponse("/rest/api/latest/plan/PROJ-BUILD/branch", HttpMethod.Get, branchesList);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/latest/plan/PROJ-BUILD/branch");
        var result = await response.Content.ReadFromJsonAsync<BambooBranchesList>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(3, result.Branch.Count);
        Assert.Equal("main", result.Branch[0].Name);
        Assert.True(result.Branch[0].Enabled);
        Assert.False(result.Branch[2].Enabled);
    }

    [Fact]
    public async Task GetBuildResults_ShouldReturnResults()
    {
        // Arrange
        var buildResultsResponse = new BambooBuildResultsResponse
        {
            Results = new BambooBuildResultsList
            {
                Size = 2,
                StartIndex = 0,
                MaxResult = 25,
                Result = new List<BambooBuildResult>
                {
                    new BambooBuildResult
                    {
                        Key = "PROJ-BUILD-100",
                        BuildNumber = 100,
                        State = "Successful",
                        BuildState = "Successful",
                        LifeCycleState = "Finished",
                        Successful = true,
                        Finished = true,
                        BuildRelativeTime = "5 minutes ago",
                        PlanName = "Build Plan",
                        ProjectName = "Test Project"
                    },
                    new BambooBuildResult
                    {
                        Key = "PROJ-BUILD-99",
                        BuildNumber = 99,
                        State = "Failed",
                        BuildState = "Failed",
                        LifeCycleState = "Finished",
                        Successful = false,
                        Finished = true,
                        BuildRelativeTime = "1 hour ago",
                        PlanName = "Build Plan",
                        ProjectName = "Test Project"
                    }
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/latest/result/PROJ-BUILD", HttpMethod.Get, buildResultsResponse);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/latest/result/PROJ-BUILD");
        var result = await response.Content.ReadFromJsonAsync<BambooBuildResultsResponse>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Results.Result.Count);
        Assert.True(result.Results.Result[0].Successful);
        Assert.False(result.Results.Result[1].Successful);
    }

    [Fact]
    public async Task GetBuildResult_ShouldReturnDetailedResult()
    {
        // Arrange
        var buildResult = new BambooBuildResult
        {
            Key = "PROJ-BUILD-100",
            BuildNumber = 100,
            BuildResultKey = "PROJ-BUILD-100",
            State = "Successful",
            BuildState = "Successful",
            LifeCycleState = "Finished",
            Successful = true,
            Finished = true,
            BuildReason = "Manual run by testuser",
            ReasonSummary = "Manual run",
            PlanName = "Build Plan",
            ProjectName = "Test Project",
            BuildStartedTime = "2024-01-15T10:00:00.000+0000",
            BuildCompletedTime = "2024-01-15T10:02:30.000+0000",
            BuildDuration = 150000,
            BuildDurationDescription = "2 minutes 30 seconds",
            BuildDurationInSeconds = 150,
            BuildRelativeTime = "5 minutes ago",
            Link = new BambooLink { Href = "http://localhost/browse/PROJ-BUILD-100" },
            SuccessfulTestCount = 50,
            FailedTestCount = 0,
            QuarantinedTestCount = 2,
            SkippedTestCount = 5,
            Stages = new BambooStageResultsList
            {
                Size = 2,
                Stage = new List<BambooStageResult>
                {
                    new BambooStageResult { Name = "Build Stage", State = "Successful", Finished = true, Successful = true },
                    new BambooStageResult { Name = "Test Stage", State = "Successful", Finished = true, Successful = true }
                }
            },
            Changes = new BambooChangesList
            {
                Size = 2,
                Change = new List<BambooChange>
                {
                    new BambooChange
                    {
                        ChangesetId = "abc123def456",
                        Author = "john.doe",
                        FullName = "John Doe",
                        Comment = "Fixed bug in authentication",
                        Date = "2024-01-15T09:55:00.000+0000"
                    },
                    new BambooChange
                    {
                        ChangesetId = "def789ghi012",
                        Author = "jane.smith",
                        FullName = "Jane Smith",
                        Comment = "Added new feature",
                        Date = "2024-01-15T09:50:00.000+0000"
                    }
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/latest/result/PROJ-BUILD-100", HttpMethod.Get, buildResult);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/latest/result/PROJ-BUILD-100");
        var result = await response.Content.ReadFromJsonAsync<BambooBuildResult>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("PROJ-BUILD-100", result.Key);
        Assert.Equal(100, result.BuildNumber);
        Assert.True(result.Successful);
        Assert.Equal(50, result.SuccessfulTestCount);
        Assert.Equal(0, result.FailedTestCount);
        Assert.Equal(2, result.Stages?.Stage.Count);
        Assert.Equal(2, result.Changes?.Change.Count);
        Assert.Equal("John Doe", result.Changes?.Change[0].FullName);
    }

    [Fact]
    public async Task QueueBuild_ShouldReturnQueueResponse()
    {
        // Arrange
        var queueResponse = new BambooQueueResponse
        {
            BuildNumber = 101,
            BuildResultKey = "PROJ-BUILD-101",
            PlanKey = "PROJ-BUILD",
            TriggerReason = "Manual run by testuser",
            Link = new BambooLink { Href = "http://localhost/browse/PROJ-BUILD-101" }
        };

        _mockHandler.SetupResponse("/rest/api/latest/queue/PROJ-BUILD", HttpMethod.Post, queueResponse);

        // Act
        var response = await _httpClient.PostAsync("/rest/api/latest/queue/PROJ-BUILD", null);
        var result = await response.Content.ReadFromJsonAsync<BambooQueueResponse>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(101, result.BuildNumber);
        Assert.Equal("PROJ-BUILD-101", result.BuildResultKey);
        Assert.Equal("PROJ-BUILD", result.PlanKey);
    }

    [Fact]
    public async Task GetProject_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/latest/project/INVALID", HttpMethod.Get, _ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"message\":\"Project 'INVALID' does not exist\"}")
            });

        // Act
        var response = await _httpClient.GetAsync("/rest/api/latest/project/INVALID");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void BambooModels_ShouldSerializeCorrectly()
    {
        // Arrange
        var project = new BambooProject
        {
            Key = "PROJ",
            Name = "Test Project",
            Description = "Test description",
            Link = new BambooLink { Href = "http://localhost/browse/PROJ", Rel = "self" }
        };

        // Act
        var json = JsonSerializer.Serialize(project);
        var deserialized = JsonSerializer.Deserialize<BambooProject>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(project.Key, deserialized.Key);
        Assert.Equal(project.Name, deserialized.Name);
        Assert.Equal(project.Link?.Href, deserialized.Link?.Href);
    }

    [Fact]
    public void BambooPlan_ShouldSerializeCorrectly()
    {
        // Arrange
        var plan = new BambooPlan
        {
            Key = "PROJ-BUILD",
            Name = "Build Plan",
            ShortKey = "BUILD",
            ShortName = "Build",
            Description = "Main build",
            ProjectKey = "PROJ",
            ProjectName = "Test Project",
            Enabled = true,
            Type = "chain",
            AverageBuildTimeInSeconds = 100.5,
            IsBuilding = false,
            IsActive = true,
            IsFavourite = false
        };

        // Act
        var json = JsonSerializer.Serialize(plan);
        var deserialized = JsonSerializer.Deserialize<BambooPlan>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(plan.Key, deserialized.Key);
        Assert.Equal(plan.Name, deserialized.Name);
        Assert.Equal(plan.ProjectKey, deserialized.ProjectKey);
        Assert.Equal(plan.Enabled, deserialized.Enabled);
        Assert.Equal(plan.AverageBuildTimeInSeconds, deserialized.AverageBuildTimeInSeconds);
    }

    [Fact]
    public void BambooBuildResult_ShouldSerializeCorrectly()
    {
        // Arrange
        var buildResult = new BambooBuildResult
        {
            Key = "PROJ-BUILD-100",
            BuildNumber = 100,
            State = "Successful",
            BuildState = "Successful",
            LifeCycleState = "Finished",
            Successful = true,
            Finished = true,
            BuildReason = "Manual run",
            BuildStartedTime = "2024-01-15T10:00:00.000+0000",
            BuildCompletedTime = "2024-01-15T10:02:00.000+0000",
            BuildDuration = 120000,
            BuildDurationInSeconds = 120,
            BuildRelativeTime = "5 minutes ago",
            SuccessfulTestCount = 50,
            FailedTestCount = 2,
            QuarantinedTestCount = 1,
            SkippedTestCount = 3
        };

        // Act
        var json = JsonSerializer.Serialize(buildResult);
        var deserialized = JsonSerializer.Deserialize<BambooBuildResult>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(buildResult.Key, deserialized.Key);
        Assert.Equal(buildResult.BuildNumber, deserialized.BuildNumber);
        Assert.Equal(buildResult.Successful, deserialized.Successful);
        Assert.Equal(buildResult.SuccessfulTestCount, deserialized.SuccessfulTestCount);
        Assert.Equal(buildResult.FailedTestCount, deserialized.FailedTestCount);
    }

    [Fact]
    public void BambooQueueResponse_ShouldSerializeCorrectly()
    {
        // Arrange
        var queueResponse = new BambooQueueResponse
        {
            BuildNumber = 101,
            BuildResultKey = "PROJ-BUILD-101",
            PlanKey = "PROJ-BUILD",
            TriggerReason = "Manual run",
            Link = new BambooLink { Href = "http://localhost/browse/PROJ-BUILD-101" }
        };

        // Act
        var json = JsonSerializer.Serialize(queueResponse);
        var deserialized = JsonSerializer.Deserialize<BambooQueueResponse>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(queueResponse.BuildNumber, deserialized.BuildNumber);
        Assert.Equal(queueResponse.BuildResultKey, deserialized.BuildResultKey);
        Assert.Equal(queueResponse.PlanKey, deserialized.PlanKey);
    }
}
