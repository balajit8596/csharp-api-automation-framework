using Qa.ApiFramework.Http;
using WireMock.Server;

namespace Qa.BddTests.Support;

/// <summary>
/// Holds state shared by the steps of one scenario. SpecFlow creates one instance per scenario
/// and injects it into any step class that asks for it in its constructor.
/// </summary>
public class ApiContext : IDisposable
{
    public string BaseUrl { get; set; } = string.Empty;

    public ApiResponse? Response { get; set; }

    public WireMockServer? Server { get; set; }

    public void Dispose()
    {
        if (Server != null)
        {
            Server.Stop();
            Server.Dispose();
            Server = null;
        }
    }
}
