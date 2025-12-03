using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace AtlassianCli.Client;

/// <summary>
/// Helper class for HTTP operations with request logging.
/// </summary>
public static class HttpClientHelper
{
    /// <summary>
    /// Logs HTTP request details to the console.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The request URL.</param>
    public static void LogRequest(HttpMethod method, string url)
    {
        Console.WriteLine($"[HTTP] {method.Method} {url}");
    }

    /// <summary>
    /// Logs HTTP response details to the console.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The request URL.</param>
    /// <param name="statusCode">The response status code.</param>
    /// <param name="elapsed">The elapsed time for the request.</param>
    public static void LogResponse(HttpMethod method, string url, int statusCode, TimeSpan elapsed)
    {
        Console.WriteLine($"[HTTP] {method.Method} {url} => {statusCode} ({elapsed.TotalMilliseconds:F0}ms)");
    }

    /// <summary>
    /// Sends an HTTP GET request with logging.
    /// </summary>
    public static async Task<HttpResponseMessage> GetAsync(HttpClient client, string url)
    {
        LogRequest(HttpMethod.Get, url);
        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync(url);
        stopwatch.Stop();
        LogResponse(HttpMethod.Get, url, (int)response.StatusCode, stopwatch.Elapsed);
        return response;
    }

    /// <summary>
    /// Sends an HTTP POST request with logging.
    /// </summary>
    public static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, HttpContent? content)
    {
        LogRequest(HttpMethod.Post, url);
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsync(url, content);
        stopwatch.Stop();
        LogResponse(HttpMethod.Post, url, (int)response.StatusCode, stopwatch.Elapsed);
        return response;
    }

    /// <summary>
    /// Sends an HTTP POST request with JSON content with logging.
    /// </summary>
    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(HttpClient client, string url, T content, JsonSerializerOptions? options = null)
    {
        LogRequest(HttpMethod.Post, url);
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync(url, content, options);
        stopwatch.Stop();
        LogResponse(HttpMethod.Post, url, (int)response.StatusCode, stopwatch.Elapsed);
        return response;
    }

    /// <summary>
    /// Sends an HTTP PUT request with logging.
    /// </summary>
    public static async Task<HttpResponseMessage> PutAsync(HttpClient client, string url, HttpContent? content)
    {
        LogRequest(HttpMethod.Put, url);
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PutAsync(url, content);
        stopwatch.Stop();
        LogResponse(HttpMethod.Put, url, (int)response.StatusCode, stopwatch.Elapsed);
        return response;
    }

    /// <summary>
    /// Sends an HTTP PUT request with JSON content with logging.
    /// </summary>
    public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(HttpClient client, string url, T content, JsonSerializerOptions? options = null)
    {
        LogRequest(HttpMethod.Put, url);
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PutAsJsonAsync(url, content, options);
        stopwatch.Stop();
        LogResponse(HttpMethod.Put, url, (int)response.StatusCode, stopwatch.Elapsed);
        return response;
    }
}
