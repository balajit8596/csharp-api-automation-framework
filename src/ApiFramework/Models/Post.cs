using Newtonsoft.Json;

namespace Qa.ApiFramework.Models;

/// <summary>Request and response model for the posts resource.</summary>
public class Post
{
    [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
    public int? UserId { get; set; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public int? Id { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string? Title { get; set; }

    [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
    public string? Body { get; set; }
}
