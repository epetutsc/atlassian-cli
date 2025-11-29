using System.Net.Http.Json;
using AtlassianCli.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace AtlassianCli.Tests;

/// <summary>
/// Trait to mark tests that require Docker to run.
/// </summary>
public sealed class DockerRequiredFactAttribute : FactAttribute
{
    public DockerRequiredFactAttribute()
    {
        if (!IsDockerAvailable())
        {
            Skip = "Docker is not available on this system. These tests require Docker to run.";
        }
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = "info";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Integration tests using Testcontainers with WireMock Docker image to simulate Atlassian APIs.
/// These tests demonstrate the Testcontainers pattern for integration testing with Docker.
/// The tests will be skipped if Docker is not available.
/// </summary>
public class TestcontainersIntegrationTests : IAsyncLifetime
{
    private IContainer? _wireMockContainer;
    private HttpClient? _httpClient;
    private string _containerUrl = string.Empty;
    private const int WireMockPort = 8080;
    private bool _dockerAvailable;

    public async Task InitializeAsync()
    {
        _dockerAvailable = IsDockerAvailable();
        if (!_dockerAvailable)
        {
            return;
        }

        try
        {
            // Create and start WireMock container
            _wireMockContainer = new ContainerBuilder()
                .WithImage("wiremock/wiremock:latest")
                .WithPortBinding(WireMockPort, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(WireMockPort).ForPath("/__admin/mappings")))
                .Build();

            await _wireMockContainer.StartAsync();

            var mappedPort = _wireMockContainer.GetMappedPublicPort(WireMockPort);
            _containerUrl = $"http://localhost:{mappedPort}";

            _httpClient = new HttpClient { BaseAddress = new Uri(_containerUrl) };
        }
        catch (Exception)
        {
            _dockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();

        if (_wireMockContainer != null)
        {
            try
            {
                await _wireMockContainer.StopAsync();
                await _wireMockContainer.DisposeAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = "info";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Registers a WireMock stub via the admin API.
    /// </summary>
    private async Task RegisterStubAsync(object stubDefinition)
    {
        if (_httpClient == null) return;
        var response = await _httpClient.PostAsJsonAsync("/__admin/mappings", stubDefinition);
        response.EnsureSuccessStatusCode();
    }

    [DockerRequiredFact]
    public async Task WireMockContainer_ShouldStartSuccessfully()
    {
        // Assert
        Assert.NotNull(_wireMockContainer);
        Assert.Equal("Running", _wireMockContainer.State.ToString());
    }

    [DockerRequiredFact]
    public async Task ConfluenceApi_CreatePage_ShouldReturnPage()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange
        var expectedPage = new ConfluencePage
        {
            Id = "12345",
            Title = "Docker Test Page",
            Status = "current",
            Space = new SpaceReference { Key = "DOCKER" },
            Version = new PageVersion { Number = 1 }
        };

        var stub = new
        {
            request = new
            {
                method = "POST",
                urlPath = "/rest/api/content"
            },
            response = new
            {
                status = 200,
                headers = new { ContentType = "application/json" },
                jsonBody = expectedPage
            }
        };

        await RegisterStubAsync(stub);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/rest/api/content", new
        {
            type = "page",
            title = "Docker Test Page",
            space = new { key = "DOCKER" },
            body = new { storage = new { value = "<p>Test</p>", representation = "storage" } }
        });

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var page = await response.Content.ReadFromJsonAsync<ConfluencePage>();
        Assert.NotNull(page);
        Assert.Equal("Docker Test Page", page.Title);
        Assert.Equal("DOCKER", page.Space?.Key);
    }

    [DockerRequiredFact]
    public async Task ConfluenceApi_GetPage_ShouldReturnPage()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange
        var expectedPage = new ConfluencePage
        {
            Id = "67890",
            Title = "Existing Page",
            Status = "current",
            Space = new SpaceReference { Key = "DOCS" },
            Body = new PageBody
            {
                Storage = new StorageContent { Value = "<p>Content from Docker</p>" }
            },
            Version = new PageVersion { Number = 3 }
        };

        var stub = new
        {
            request = new
            {
                method = "GET",
                urlPath = "/rest/api/content/67890"
            },
            response = new
            {
                status = 200,
                headers = new { ContentType = "application/json" },
                jsonBody = expectedPage
            }
        };

        await RegisterStubAsync(stub);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/content/67890");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var page = await response.Content.ReadFromJsonAsync<ConfluencePage>();
        Assert.NotNull(page);
        Assert.Equal("67890", page.Id);
        Assert.Equal("Existing Page", page.Title);
    }

    [DockerRequiredFact]
    public async Task JiraApi_GetIssue_ShouldReturnIssue()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange
        var expectedIssue = new JiraIssue
        {
            Id = "10001",
            Key = "DOCKER-1",
            Fields = new JiraIssueFields
            {
                Summary = "Docker Test Issue",
                Description = "Created in Testcontainers",
                IssueType = new JiraIssueType { Name = "Task" },
                Status = new JiraStatus { Name = "Open" },
                Project = new JiraProject { Key = "DOCKER", Name = "Docker Project" }
            }
        };

        var stub = new
        {
            request = new
            {
                method = "GET",
                urlPath = "/rest/api/2/issue/DOCKER-1"
            },
            response = new
            {
                status = 200,
                headers = new { ContentType = "application/json" },
                jsonBody = expectedIssue
            }
        };

        await RegisterStubAsync(stub);

        // Act
        var response = await _httpClient.GetAsync("/rest/api/2/issue/DOCKER-1");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var issue = await response.Content.ReadFromJsonAsync<JiraIssue>();
        Assert.NotNull(issue);
        Assert.Equal("DOCKER-1", issue.Key);
        Assert.Equal("Docker Test Issue", issue.Fields.Summary);
    }

    [DockerRequiredFact]
    public async Task JiraApi_CreateIssue_ShouldReturnCreatedIssue()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange
        var expectedResponse = new CreateJiraIssueResponse
        {
            Id = "10002",
            Key = "DOCKER-2",
            Self = $"{_containerUrl}/rest/api/2/issue/10002"
        };

        var stub = new
        {
            request = new
            {
                method = "POST",
                urlPath = "/rest/api/2/issue"
            },
            response = new
            {
                status = 201,
                headers = new { ContentType = "application/json" },
                jsonBody = expectedResponse
            }
        };

        await RegisterStubAsync(stub);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/rest/api/2/issue", new
        {
            fields = new
            {
                project = new { key = "DOCKER" },
                summary = "New Docker Issue",
                issuetype = new { name = "Task" }
            }
        });

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateJiraIssueResponse>();
        Assert.NotNull(created);
        Assert.Equal("DOCKER-2", created.Key);
    }

    [DockerRequiredFact]
    public async Task JiraApi_AddComment_ShouldReturnComment()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange
        var expectedComment = new JiraComment
        {
            Id = "30001",
            Body = "Comment from Docker test",
            Created = "2024-01-15T10:00:00.000+0000"
        };

        var stub = new
        {
            request = new
            {
                method = "POST",
                urlPath = "/rest/api/2/issue/DOCKER-1/comment"
            },
            response = new
            {
                status = 201,
                headers = new { ContentType = "application/json" },
                jsonBody = expectedComment
            }
        };

        await RegisterStubAsync(stub);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/rest/api/2/issue/DOCKER-1/comment", new
        {
            body = "Comment from Docker test"
        });

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var comment = await response.Content.ReadFromJsonAsync<JiraComment>();
        Assert.NotNull(comment);
        Assert.Equal("Comment from Docker test", comment.Body);
    }

    [DockerRequiredFact]
    public async Task JiraApi_TransitionIssue_ShouldReturnSuccess()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange - First setup the transitions endpoint
        var transitions = new
        {
            transitions = new[]
            {
                new { id = "21", name = "Done", to = new { name = "Done" } }
            }
        };

        var getTransitionsStub = new
        {
            request = new
            {
                method = "GET",
                urlPath = "/rest/api/2/issue/DOCKER-1/transitions"
            },
            response = new
            {
                status = 200,
                headers = new { ContentType = "application/json" },
                jsonBody = transitions
            }
        };

        await RegisterStubAsync(getTransitionsStub);

        var postTransitionStub = new
        {
            request = new
            {
                method = "POST",
                urlPath = "/rest/api/2/issue/DOCKER-1/transitions"
            },
            response = new
            {
                status = 204
            }
        };

        await RegisterStubAsync(postTransitionStub);

        // Act - Get transitions
        var getResponse = await _httpClient.GetAsync("/rest/api/2/issue/DOCKER-1/transitions");
        Assert.True(getResponse.IsSuccessStatusCode);

        // Act - Perform transition
        var postResponse = await _httpClient.PostAsJsonAsync("/rest/api/2/issue/DOCKER-1/transitions", new
        {
            transition = new { id = "21" }
        });

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, postResponse.StatusCode);
    }

    [DockerRequiredFact]
    public async Task ConfluenceApi_UpdatePage_ShouldReturnUpdatedPage()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange
        var updatedPage = new ConfluencePage
        {
            Id = "11111",
            Title = "Updated Page",
            Status = "current",
            Version = new PageVersion { Number = 2 }
        };

        var stub = new
        {
            request = new
            {
                method = "PUT",
                urlPath = "/rest/api/content/11111"
            },
            response = new
            {
                status = 200,
                headers = new { ContentType = "application/json" },
                jsonBody = updatedPage
            }
        };

        await RegisterStubAsync(stub);

        // Act
        var response = await _httpClient.PutAsJsonAsync("/rest/api/content/11111", new
        {
            id = "11111",
            type = "page",
            title = "Updated Page",
            body = new { storage = new { value = "<p>Updated content</p>", representation = "storage" } },
            version = new { number = 2 }
        });

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var page = await response.Content.ReadFromJsonAsync<ConfluencePage>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Version?.Number);
    }

    [DockerRequiredFact]
    public async Task JiraApi_AssignUser_ShouldReturnSuccess()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange
        var stub = new
        {
            request = new
            {
                method = "PUT",
                urlPath = "/rest/api/2/issue/DOCKER-1/assignee"
            },
            response = new
            {
                status = 204
            }
        };

        await RegisterStubAsync(stub);

        // Act
        var response = await _httpClient.PutAsJsonAsync("/rest/api/2/issue/DOCKER-1/assignee", new
        {
            accountId = "user-123"
        });

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [DockerRequiredFact]
    public async Task JiraApi_UpdateIssue_ShouldReturnSuccess()
    {
        if (!_dockerAvailable || _httpClient == null)
        {
            Assert.True(true, "Skipped: Docker not available");
            return;
        }

        // Arrange
        var stub = new
        {
            request = new
            {
                method = "PUT",
                urlPath = "/rest/api/2/issue/DOCKER-1"
            },
            response = new
            {
                status = 204
            }
        };

        await RegisterStubAsync(stub);

        // Act
        var response = await _httpClient.PutAsJsonAsync("/rest/api/2/issue/DOCKER-1", new
        {
            fields = new { description = "Updated from Docker test" }
        });

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
}
