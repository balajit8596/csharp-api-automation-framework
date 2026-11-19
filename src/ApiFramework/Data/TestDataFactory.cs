using Qa.ApiFramework.Models;

namespace Qa.ApiFramework.Data;

/// <summary>Builds unique test data so runs never collide with each other.</summary>
public static class TestDataFactory
{
    public static Post NewPost() => new()
    {
        UserId = 1,
        Title = $"qa-post-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
        Body = "Created by the C# API automation framework"
    };
}
