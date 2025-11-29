using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AtlassianCli.Models;

namespace AtlassianCli.Tests;

/// <summary>
/// A mock HttpMessageHandler that simulates Jira REST API responses.
/// </summary>
public class MockJiraHandler : HttpMessageHandler
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
/// Integration tests for Jira CLI commands using a mock HTTP handler.
/// </summary>
public class JiraIntegrationTests : IDisposable
{
    private readonly MockJiraHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly string _originalBaseUrl;
    private readonly string _originalUsername;
    private readonly string _originalApiToken;

    public JiraIntegrationTests()
    {
        // Save original environment variables
        _originalBaseUrl = Environment.GetEnvironmentVariable("JIRA_BASE_URL") ?? string.Empty;
        _originalUsername = Environment.GetEnvironmentVariable("JIRA_USERNAME") ?? string.Empty;
        _originalApiToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN") ?? string.Empty;

        _mockHandler = new MockJiraHandler();
        _httpClient = new HttpClient(_mockHandler) { BaseAddress = new Uri("http://localhost") };

        // Set up environment variables for the test
        Environment.SetEnvironmentVariable("JIRA_BASE_URL", "http://localhost");
        Environment.SetEnvironmentVariable("JIRA_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("JIRA_API_TOKEN", "test-api-token");
    }

    public void Dispose()
    {
        // Restore original environment variables
        if (string.IsNullOrEmpty(_originalBaseUrl))
            Environment.SetEnvironmentVariable("JIRA_BASE_URL", null);
        else
            Environment.SetEnvironmentVariable("JIRA_BASE_URL", _originalBaseUrl);

        if (string.IsNullOrEmpty(_originalUsername))
            Environment.SetEnvironmentVariable("JIRA_USERNAME", null);
        else
            Environment.SetEnvironmentVariable("JIRA_USERNAME", _originalUsername);

        if (string.IsNullOrEmpty(_originalApiToken))
            Environment.SetEnvironmentVariable("JIRA_API_TOKEN", null);
        else
            Environment.SetEnvironmentVariable("JIRA_API_TOKEN", _originalApiToken);

        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [Fact]
    public async Task GetIssue_ShouldReturnIssue()
    {
        // Arrange
        var expectedIssue = new JiraIssue
        {
            Id = "10001",
            Key = "TEST-123",
            Self = "http://localhost/rest/api/2/issue/10001",
            Fields = new JiraIssueFields
            {
                Summary = "Test Issue",
                Description = "This is a test issue",
                IssueType = new JiraIssueType { Id = "1", Name = "Task" },
                Project = new JiraProject { Id = "10000", Key = "TEST", Name = "Test Project" },
                Status = new JiraStatus { Id = "1", Name = "To Do" },
                Priority = new JiraPriority { Id = "3", Name = "Medium" },
                Created = "2024-01-01T10:00:00.000+0000",
                Updated = "2024-01-02T15:30:00.000+0000"
            }
        };

        _mockHandler.SetupResponse("/rest/api/2/issue/TEST-123", HttpMethod.Get, expectedIssue);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/2/issue/TEST-123");
        var result = await response.Content.ReadFromJsonAsync<JiraIssue>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("TEST-123", result.Key);
        Assert.Equal("Test Issue", result.Fields.Summary);
        Assert.Equal("Task", result.Fields.IssueType?.Name);
        Assert.Equal("To Do", result.Fields.Status?.Name);
    }

    [Fact]
    public async Task GetIssue_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/2/issue/INVALID-999", HttpMethod.Get, _ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"errorMessages\":[\"Issue does not exist or you do not have permission to see it.\"]}")
            });

        // Act
        var response = await _httpClient.GetAsync("/rest/api/2/issue/INVALID-999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateIssue_ShouldReturnCreatedIssue()
    {
        // Arrange
        var expectedResponse = new CreateJiraIssueResponse
        {
            Id = "10002",
            Key = "TEST-124",
            Self = "http://localhost/rest/api/2/issue/10002"
        };

        _mockHandler.SetupResponse("/rest/api/2/issue", HttpMethod.Post, expectedResponse, HttpStatusCode.Created);

        // Act
        var request = new CreateJiraIssueRequest
        {
            Fields = new CreateJiraIssueFields
            {
                Project = new ProjectKey { Key = "TEST" },
                Summary = "New Task",
                IssueType = new IssueTypeName { Name = "Task" },
                Description = "This is a description"
            }
        };
        var response = await _httpClient.PostAsJsonAsync("/rest/api/2/issue", request);
        var result = await response.Content.ReadFromJsonAsync<CreateJiraIssueResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("TEST-124", result.Key);
        Assert.Equal("10002", result.Id);
    }

    [Fact]
    public async Task CreateIssue_WhenProjectNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/2/issue", HttpMethod.Post, _ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"errorMessages\":[],\"errors\":{\"project\":\"Project 'INVALID' does not exist.\"}}")
            });

        // Act
        var request = new CreateJiraIssueRequest
        {
            Fields = new CreateJiraIssueFields
            {
                Project = new ProjectKey { Key = "INVALID" },
                Summary = "Test",
                IssueType = new IssueTypeName { Name = "Task" }
            }
        };
        var response = await _httpClient.PostAsJsonAsync("/rest/api/2/issue", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddComment_ShouldReturnCreatedComment()
    {
        // Arrange
        var expectedComment = new JiraComment
        {
            Id = "20001",
            Body = "This is a test comment",
            Author = new JiraUser { DisplayName = "Test User" },
            Created = "2024-01-03T12:00:00.000+0000"
        };

        _mockHandler.SetupResponse("/rest/api/2/issue/TEST-123/comment", HttpMethod.Post, expectedComment, HttpStatusCode.Created);

        // Act
        var request = new AddJiraCommentRequest { Body = "This is a test comment" };
        var response = await _httpClient.PostAsJsonAsync("/rest/api/2/issue/TEST-123/comment", request);
        var result = await response.Content.ReadFromJsonAsync<JiraComment>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("20001", result.Id);
        Assert.Equal("This is a test comment", result.Body);
    }

    [Fact]
    public async Task GetTransitions_ShouldReturnTransitions()
    {
        // Arrange
        var transitions = new JiraTransitionsResponse
        {
            Transitions = new List<JiraTransition>
            {
                new JiraTransition { Id = "11", Name = "Start Progress", To = new JiraStatus { Name = "In Progress" } },
                new JiraTransition { Id = "21", Name = "Done", To = new JiraStatus { Name = "Done" } }
            }
        };

        _mockHandler.SetupResponse("/rest/api/2/issue/TEST-123/transitions", HttpMethod.Get, transitions);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/2/issue/TEST-123/transitions");
        var result = await response.Content.ReadFromJsonAsync<JiraTransitionsResponse>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Transitions.Count);
        Assert.Equal("Start Progress", result.Transitions[0].Name);
    }

    [Fact]
    public async Task TransitionIssue_ShouldReturnNoContent()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/2/issue/TEST-123/transitions", HttpMethod.Post, _ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        // Act
        var request = new TransitionJiraIssueRequest { Transition = new TransitionId { Id = "21" } };
        var response = await _httpClient.PostAsJsonAsync("/rest/api/2/issue/TEST-123/transitions", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SearchUsers_ShouldReturnUsers()
    {
        // Arrange
        var users = new List<JiraUserSearchResult>
        {
            new JiraUserSearchResult
            {
                AccountId = "user-account-id-123",
                Name = "john.doe",
                DisplayName = "John Doe",
                Active = true
            }
        };

        _mockHandler.SetupResponse("/rest/api/2/user/search", HttpMethod.Get, users);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/2/user/search?query=john.doe");
        var result = await response.Content.ReadFromJsonAsync<List<JiraUserSearchResult>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("John Doe", result[0].DisplayName);
    }

    [Fact]
    public async Task AssignIssue_ShouldReturnNoContent()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/2/issue/TEST-123/assignee", HttpMethod.Put, _ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        // Act
        var request = new AssignJiraIssueRequest { AccountId = "user-account-id-123" };
        var response = await _httpClient.PutAsJsonAsync("/rest/api/2/issue/TEST-123/assignee", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateIssueDescription_ShouldReturnNoContent()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/2/issue/TEST-123", HttpMethod.Put, _ =>
            new HttpResponseMessage(HttpStatusCode.NoContent));

        // Act
        var request = new UpdateJiraIssueRequest
        {
            Fields = new UpdateJiraIssueFields { Description = "Updated description text" }
        };
        var response = await _httpClient.PutAsJsonAsync("/rest/api/2/issue/TEST-123", request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetIssue_WithComments_ShouldReturnIssueWithComments()
    {
        // Arrange
        var expectedIssue = new JiraIssue
        {
            Id = "10001",
            Key = "TEST-123",
            Fields = new JiraIssueFields
            {
                Summary = "Issue with Comments",
                Description = "Test description",
                IssueType = new JiraIssueType { Name = "Bug" },
                Status = new JiraStatus { Name = "Open" },
                Comment = new JiraCommentContainer
                {
                    Total = 2,
                    Comments = new List<JiraComment>
                    {
                        new JiraComment
                        {
                            Id = "1",
                            Body = "First comment",
                            Author = new JiraUser { DisplayName = "User1" },
                            Created = "2024-01-01T10:00:00.000+0000"
                        },
                        new JiraComment
                        {
                            Id = "2",
                            Body = "Second comment",
                            Author = new JiraUser { DisplayName = "User2" },
                            Created = "2024-01-02T10:00:00.000+0000"
                        }
                    }
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/2/issue/TEST-123", HttpMethod.Get, expectedIssue);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/2/issue/TEST-123");
        var result = await response.Content.ReadFromJsonAsync<JiraIssue>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.NotNull(result.Fields.Comment);
        Assert.Equal(2, result.Fields.Comment.Total);
        Assert.Equal(2, result.Fields.Comment.Comments.Count);
        Assert.Equal("First comment", result.Fields.Comment.Comments[0].Body);
    }

    [Fact]
    public async Task GetIssue_WithAssigneeAndReporter_ShouldReturnUsers()
    {
        // Arrange
        var expectedIssue = new JiraIssue
        {
            Id = "10001",
            Key = "TEST-123",
            Fields = new JiraIssueFields
            {
                Summary = "Issue with Users",
                IssueType = new JiraIssueType { Name = "Story" },
                Status = new JiraStatus { Name = "In Progress" },
                Assignee = new JiraUser
                {
                    AccountId = "account-123",
                    DisplayName = "John Doe",
                    EmailAddress = "john.doe@example.com",
                    Active = true
                },
                Reporter = new JiraUser
                {
                    AccountId = "account-456",
                    DisplayName = "Jane Smith",
                    EmailAddress = "jane.smith@example.com",
                    Active = true
                }
            }
        };

        _mockHandler.SetupResponse("/rest/api/2/issue/TEST-123", HttpMethod.Get, expectedIssue);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/2/issue/TEST-123");
        var result = await response.Content.ReadFromJsonAsync<JiraIssue>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.NotNull(result.Fields.Assignee);
        Assert.Equal("John Doe", result.Fields.Assignee.DisplayName);
        Assert.NotNull(result.Fields.Reporter);
        Assert.Equal("Jane Smith", result.Fields.Reporter.DisplayName);
    }

    [Fact]
    public void JiraModels_ShouldSerializeCorrectly()
    {
        // Arrange
        var issue = new JiraIssue
        {
            Id = "10001",
            Key = "TEST-123",
            Self = "http://localhost/rest/api/2/issue/10001",
            Fields = new JiraIssueFields
            {
                Summary = "Test Issue",
                Description = "Test description",
                IssueType = new JiraIssueType { Id = "1", Name = "Task", Description = "A task" },
                Project = new JiraProject { Id = "10000", Key = "TEST", Name = "Test Project" },
                Status = new JiraStatus 
                { 
                    Id = "1", 
                    Name = "Open", 
                    Description = "Open status",
                    StatusCategory = new JiraStatusCategory { Id = 1, Key = "new", Name = "To Do" }
                },
                Priority = new JiraPriority { Id = "3", Name = "Medium" },
                Assignee = new JiraUser { AccountId = "acc-1", DisplayName = "John Doe", Active = true },
                Reporter = new JiraUser { AccountId = "acc-2", DisplayName = "Jane Smith", Active = true },
                Created = "2024-01-01T10:00:00.000+0000",
                Updated = "2024-01-02T15:30:00.000+0000",
                Comment = new JiraCommentContainer
                {
                    Total = 1,
                    Comments = new List<JiraComment>
                    {
                        new JiraComment { Id = "1", Body = "Comment", Created = "2024-01-01T12:00:00.000+0000" }
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(issue);
        var deserialized = JsonSerializer.Deserialize<JiraIssue>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(issue.Key, deserialized.Key);
        Assert.Equal(issue.Fields.Summary, deserialized.Fields.Summary);
        Assert.Equal(issue.Fields.IssueType?.Name, deserialized.Fields.IssueType?.Name);
        Assert.Equal(issue.Fields.Status?.Name, deserialized.Fields.Status?.Name);
        Assert.Equal(issue.Fields.Assignee?.DisplayName, deserialized.Fields.Assignee?.DisplayName);
    }

    [Fact]
    public void CreateJiraIssueRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new CreateJiraIssueRequest
        {
            Fields = new CreateJiraIssueFields
            {
                Project = new ProjectKey { Key = "TEST" },
                Summary = "New Issue",
                IssueType = new IssueTypeName { Name = "Task" },
                Description = "Test description"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<CreateJiraIssueRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(request.Fields.Project.Key, deserialized.Fields.Project.Key);
        Assert.Equal(request.Fields.Summary, deserialized.Fields.Summary);
        Assert.Equal(request.Fields.IssueType.Name, deserialized.Fields.IssueType.Name);
    }

    [Fact]
    public void TransitionRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new TransitionJiraIssueRequest
        {
            Transition = new TransitionId { Id = "21" }
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<TransitionJiraIssueRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(request.Transition.Id, deserialized.Transition.Id);
    }

    [Fact]
    public void AssignRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new AssignJiraIssueRequest
        {
            AccountId = "account-123",
            Name = "john.doe"
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<AssignJiraIssueRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(request.AccountId, deserialized.AccountId);
        Assert.Equal(request.Name, deserialized.Name);
    }
}
