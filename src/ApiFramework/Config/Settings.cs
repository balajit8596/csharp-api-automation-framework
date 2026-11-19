using Microsoft.Extensions.Configuration;

namespace Qa.ApiFramework.Config;

/// <summary>
/// Central configuration. Values come from appsettings.json and can be overridden by environment
/// variables with the same name, for example ApiBaseUrl=https://staging.example.com.
/// </summary>
public static class Settings
{
    private static readonly IConfigurationRoot Root = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    public static string ApiBaseUrl => Root["ApiBaseUrl"] ?? "https://jsonplaceholder.typicode.com";

    public static long MaxResponseMs => long.TryParse(Root["MaxResponseMs"], out var value) ? value : 3000;
}
