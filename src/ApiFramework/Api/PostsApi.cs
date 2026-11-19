using Qa.ApiFramework.Http;
using Qa.ApiFramework.Models;

namespace Qa.ApiFramework.Api;

/// <summary>
/// Service object for the posts endpoints. It sends requests and returns the response.
/// Assertions stay in the tests, so one method serves positive and negative cases.
/// </summary>
public sealed class PostsApi : IDisposable
{
    private readonly ApiClient _client;

    public PostsApi(string baseUrl)
    {
        _client = new ApiClient(baseUrl);
    }

    public Task<ApiResponse> GetAllAsync() => _client.GetAsync("/posts");

    public Task<ApiResponse> GetByIdAsync(int id) => _client.GetAsync($"/posts/{id}");

    public Task<ApiResponse> GetByUserAsync(int userId)
        => _client.GetAsync("/posts", new Dictionary<string, string> { ["userId"] = userId.ToString() });

    public Task<ApiResponse> CreateAsync(Post post) => _client.PostAsync("/posts", post);

    public Task<ApiResponse> UpdateAsync(int id, Post post) => _client.PutAsync($"/posts/{id}", post);

    public Task<ApiResponse> DeleteAsync(int id) => _client.DeleteAsync($"/posts/{id}");

    public void Dispose() => _client.Dispose();
}
