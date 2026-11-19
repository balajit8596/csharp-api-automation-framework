using System.Diagnostics;
using Newtonsoft.Json;
using RestSharp;

namespace Qa.ApiFramework.Http;

/// <summary>
/// Thin wrapper around RestSharp. Tests call GetAsync, PostAsync and so on, and never touch
/// RestSharp directly, so the HTTP library can change in one place.
/// </summary>
public sealed class ApiClient : IDisposable
{
    private readonly RestClient _client;

    public ApiClient(string baseUrl)
    {
        _client = new RestClient(new RestClientOptions(baseUrl));
    }

    public Task<ApiResponse> GetAsync(string resource, IDictionary<string, string>? query = null)
        => SendAsync(Method.Get, resource, null, query);

    public Task<ApiResponse> PostAsync(string resource, object body)
        => SendAsync(Method.Post, resource, body, null);

    public Task<ApiResponse> PutAsync(string resource, object body)
        => SendAsync(Method.Put, resource, body, null);

    public Task<ApiResponse> DeleteAsync(string resource)
        => SendAsync(Method.Delete, resource, null, null);

    private async Task<ApiResponse> SendAsync(
        Method method,
        string resource,
        object? body,
        IDictionary<string, string>? query)
    {
        var request = new RestRequest(resource, method);
        request.AddHeader("Accept", "application/json");

        if (query != null)
        {
            foreach (var pair in query)
            {
                request.AddQueryParameter(pair.Key, pair.Value);
            }
        }

        if (body != null)
        {
            request.AddStringBody(JsonConvert.SerializeObject(body), DataFormat.Json);
        }

        var timer = Stopwatch.StartNew();
        var response = await _client.ExecuteAsync(request);
        timer.Stop();

        return new ApiResponse((int)response.StatusCode, response.Content ?? string.Empty, timer.ElapsedMilliseconds);
    }

    public void Dispose() => _client.Dispose();
}
