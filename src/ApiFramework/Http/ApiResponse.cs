using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Qa.ApiFramework.Http;

/// <summary>What a test needs from a response: status, body and how long it took.</summary>
public sealed class ApiResponse
{
    public ApiResponse(int statusCode, string content, long elapsedMs)
    {
        StatusCode = statusCode;
        Content = content;
        ElapsedMs = elapsedMs;
    }

    public int StatusCode { get; }

    public string Content { get; }

    public long ElapsedMs { get; }

    /// <summary>Deserialize the body into a model.</summary>
    public T? As<T>() => JsonConvert.DeserializeObject<T>(Content);

    /// <summary>Parse the body as a JSON object.</summary>
    public JObject Json() => JObject.Parse(Content);

    /// <summary>Parse the body as a JSON array.</summary>
    public JArray JsonArray() => JArray.Parse(Content);
}
