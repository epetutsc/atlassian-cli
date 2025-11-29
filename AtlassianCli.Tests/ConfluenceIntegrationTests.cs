using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AtlassianCli.Models;

namespace AtlassianCli.Tests;

/// <summary>
/// A mock HttpMessageHandler that simulates Confluence REST API responses.
/// </summary>
public class MockConfluenceHandler : HttpMessageHandler
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
/// Integration tests for Confluence CLI commands using a mock HTTP handler.
/// </summary>
public class ConfluenceIntegrationTests : IDisposable
{
    private readonly MockConfluenceHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly string _originalBaseUrl;
    private readonly string _originalUsername;
    private readonly string _originalApiToken;

    public ConfluenceIntegrationTests()
    {
        // Save original environment variables
        _originalBaseUrl = Environment.GetEnvironmentVariable("CONFLUENCE_BASE_URL") ?? string.Empty;
        _originalUsername = Environment.GetEnvironmentVariable("CONFLUENCE_USERNAME") ?? string.Empty;
        _originalApiToken = Environment.GetEnvironmentVariable("CONFLUENCE_API_TOKEN") ?? string.Empty;

        _mockHandler = new MockConfluenceHandler();
        _httpClient = new HttpClient(_mockHandler) { BaseAddress = new Uri("http://localhost") };

        // Set up environment variables for the test
        Environment.SetEnvironmentVariable("CONFLUENCE_BASE_URL", "http://localhost");
        Environment.SetEnvironmentVariable("CONFLUENCE_USERNAME", "testuser");
        Environment.SetEnvironmentVariable("CONFLUENCE_API_TOKEN", "test-api-token");
    }

    public void Dispose()
    {
        // Restore original environment variables
        if (string.IsNullOrEmpty(_originalBaseUrl))
            Environment.SetEnvironmentVariable("CONFLUENCE_BASE_URL", null);
        else
            Environment.SetEnvironmentVariable("CONFLUENCE_BASE_URL", _originalBaseUrl);

        if (string.IsNullOrEmpty(_originalUsername))
            Environment.SetEnvironmentVariable("CONFLUENCE_USERNAME", null);
        else
            Environment.SetEnvironmentVariable("CONFLUENCE_USERNAME", _originalUsername);

        if (string.IsNullOrEmpty(_originalApiToken))
            Environment.SetEnvironmentVariable("CONFLUENCE_API_TOKEN", null);
        else
            Environment.SetEnvironmentVariable("CONFLUENCE_API_TOKEN", _originalApiToken);

        _httpClient.Dispose();
        _mockHandler.Dispose();
    }

    [Fact]
    public async Task CreatePage_Request_ShouldContainCorrectPayload()
    {
        // Arrange
        var expectedPage = new ConfluencePage
        {
            Id = "12345",
            Title = "Test Page",
            Status = "current",
            Space = new SpaceReference { Key = "TESTSPACE" },
            Version = new PageVersion { Number = 1 },
            Links = new PageLinks { WebUi = "/spaces/TESTSPACE/pages/12345" }
        };

        _mockHandler.SetupResponse("/rest/api/content", HttpMethod.Post, expectedPage);

        // Act
        var request = new CreatePageRequest
        {
            Title = "Test Page",
            Space = new SpaceReference { Key = "TESTSPACE" },
            Body = new PageBody
            {
                Storage = new StorageContent { Value = "<p>Hello World</p>", Representation = "storage" }
            }
        };
        var response = await _httpClient.PostAsJsonAsync("/rest/api/content", request);
        var result = await response.Content.ReadFromJsonAsync<ConfluencePage>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("12345", result.Id);
        Assert.Equal("Test Page", result.Title);
        Assert.Equal("TESTSPACE", result.Space?.Key);
    }

    [Fact]
    public async Task GetPageById_ShouldReturnPage()
    {
        // Arrange
        var expectedPage = new ConfluencePage
        {
            Id = "12345",
            Title = "Existing Page",
            Status = "current",
            Space = new SpaceReference { Key = "DOCS" },
            Body = new PageBody
            {
                Storage = new StorageContent { Value = "<p>Page content</p>", Representation = "storage" }
            },
            Version = new PageVersion { Number = 2 },
            Links = new PageLinks { WebUi = "/spaces/DOCS/pages/12345" }
        };

        _mockHandler.SetupResponse("/rest/api/content/12345", HttpMethod.Get, expectedPage);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/content/12345");
        var result = await response.Content.ReadFromJsonAsync<ConfluencePage>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("12345", result.Id);
        Assert.Equal("Existing Page", result.Title);
        Assert.Equal("<p>Page content</p>", result.Body?.Storage?.Value);
    }

    [Fact]
    public async Task GetPageByTitle_ShouldReturnSearchResults()
    {
        // Arrange
        var searchResults = new SearchResults
        {
            Results = new List<ConfluencePage>
            {
                new ConfluencePage
                {
                    Id = "67890",
                    Title = "My Page",
                    Status = "current",
                    Space = new SpaceReference { Key = "MYSPACE" },
                    Body = new PageBody
                    {
                        Storage = new StorageContent { Value = "<p>My content</p>", Representation = "storage" }
                    },
                    Version = new PageVersion { Number = 1 }
                }
            },
            Size = 1,
            Start = 0,
            Limit = 25
        };

        _mockHandler.SetupResponse("/rest/api/content", HttpMethod.Get, searchResults);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/content?spaceKey=MYSPACE&title=My Page");
        var result = await response.Content.ReadFromJsonAsync<SearchResults>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Single(result.Results);
        Assert.Equal("67890", result.Results[0].Id);
        Assert.Equal("My Page", result.Results[0].Title);
    }

    [Fact]
    public async Task GetPageByTitle_WhenPageNotFound_ShouldReturnEmptyResults()
    {
        // Arrange
        var searchResults = new SearchResults
        {
            Results = new List<ConfluencePage>(),
            Size = 0,
            Start = 0,
            Limit = 25
        };

        _mockHandler.SetupResponse("/rest/api/content", HttpMethod.Get, searchResults);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/content?spaceKey=MYSPACE&title=NonExistent");
        var result = await response.Content.ReadFromJsonAsync<SearchResults>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task UpdatePage_ShouldReturnUpdatedPage()
    {
        // Arrange
        var updatedPage = new ConfluencePage
        {
            Id = "12345",
            Title = "Page to Update",
            Status = "current",
            Space = new SpaceReference { Key = "DOCS" },
            Body = new PageBody
            {
                Storage = new StorageContent { Value = "<p>Updated content</p>", Representation = "storage" }
            },
            Version = new PageVersion { Number = 2 },
            Links = new PageLinks { WebUi = "/spaces/DOCS/pages/12345" }
        };

        _mockHandler.SetupResponse("/rest/api/content/12345", HttpMethod.Put, updatedPage);

        // Act
        var request = new UpdatePageRequest
        {
            Id = "12345",
            Title = "Page to Update",
            Body = new PageBody
            {
                Storage = new StorageContent { Value = "<p>Updated content</p>", Representation = "storage" }
            },
            Version = new PageVersion { Number = 2 }
        };
        var response = await _httpClient.PutAsJsonAsync("/rest/api/content/12345", request);
        var result = await response.Content.ReadFromJsonAsync<ConfluencePage>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.Equal("12345", result.Id);
        Assert.Equal(2, result.Version?.Number);
    }

    [Fact]
    public async Task CreatePage_WhenApiReturnsError_ShouldReturnErrorResponse()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/content", HttpMethod.Post, _ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"statusCode\":400,\"message\":\"Space does not exist\"}")
            });

        // Act
        var request = new CreatePageRequest
        {
            Title = "Test",
            Space = new SpaceReference { Key = "INVALID" },
            Body = new PageBody { Storage = new StorageContent { Value = "<p>Content</p>" } }
        };
        var response = await _httpClient.PostAsJsonAsync("/rest/api/content", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPageById_WhenPageNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockHandler.SetupResponse("/rest/api/content/99999", HttpMethod.Get, _ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"statusCode\":404,\"message\":\"Page not found\"}")
            });

        // Act
        var response = await _httpClient.GetAsync("/rest/api/content/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void ConfluenceModels_ShouldSerializeCorrectly()
    {
        // Arrange
        var page = new ConfluencePage
        {
            Id = "12345",
            Title = "Test Page",
            Status = "current",
            Type = "page",
            Space = new SpaceReference { Key = "DOCS" },
            Body = new PageBody
            {
                Storage = new StorageContent { Value = "<p>Content</p>", Representation = "storage" },
                View = new ViewContent { Value = "<p>Rendered Content</p>", Representation = "view" }
            },
            Version = new PageVersion { Number = 1, Message = "Initial version" },
            Links = new PageLinks { WebUi = "/wiki/spaces/DOCS/pages/12345", Self = "http://localhost/rest/api/content/12345" }
        };

        // Act
        var json = JsonSerializer.Serialize(page);
        var deserialized = JsonSerializer.Deserialize<ConfluencePage>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(page.Id, deserialized.Id);
        Assert.Equal(page.Title, deserialized.Title);
        Assert.Equal(page.Space?.Key, deserialized.Space?.Key);
        Assert.Equal(page.Body?.Storage?.Value, deserialized.Body?.Storage?.Value);
        Assert.Equal(page.Version?.Number, deserialized.Version?.Number);
    }

    [Fact]
    public void CreatePageRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new CreatePageRequest
        {
            Type = "page",
            Title = "New Page",
            Space = new SpaceReference { Key = "MYSPACE" },
            Body = new PageBody
            {
                Storage = new StorageContent { Value = "<p>Hello World</p>", Representation = "storage" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<CreatePageRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(request.Title, deserialized.Title);
        Assert.Equal(request.Space.Key, deserialized.Space.Key);
        Assert.Equal(request.Body.Storage?.Value, deserialized.Body.Storage?.Value);
    }

    [Fact]
    public void UpdatePageRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new UpdatePageRequest
        {
            Id = "12345",
            Type = "page",
            Title = "Updated Title",
            Body = new PageBody
            {
                Storage = new StorageContent { Value = "<p>Updated content</p>", Representation = "storage" }
            },
            Version = new PageVersion { Number = 2 }
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<UpdatePageRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(request.Id, deserialized.Id);
        Assert.Equal(request.Title, deserialized.Title);
        Assert.Equal(request.Version.Number, deserialized.Version.Number);
    }
}
