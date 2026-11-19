using NUnit.Framework;
using Qa.ApiFramework.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Qa.ApiTests;

/// <summary>
/// Service virtualization: a local stand-in service replaces the real one. This lets us test
/// error codes and slow responses that are hard to trigger on a real environment. Runs offline.
/// (Parasoft Virtualize plays the same role in a Parasoft setup.)
/// </summary>
[TestFixture]
[Category("virtual")]
public class VirtualizedRestTests
{
    private WireMockServer _server = null!;
    private ApiClient _client = null!;

    [OneTimeSetUp]
    public void StartVirtualService()
    {
        _server = WireMockServer.Start();

        Stub("/users/1", 200, "{\"id\":1,\"name\":\"Test User\",\"active\":true}");
        Stub("/users/401", 401, "{\"error\":\"Unauthorized\"}");
        Stub("/users/500", 500, "{\"error\":\"Internal Server Error\"}");

        _server
            .Given(Request.Create().WithPath("/users/slow").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\":99,\"name\":\"Slow User\"}")
                .WithDelay(1500));

        _client = new ApiClient(_server.Urls[0]);
    }

    [OneTimeTearDown]
    public void StopVirtualService()
    {
        _client.Dispose();
        _server.Stop();
        _server.Dispose();
    }

    private void Stub(string path, int status, string body)
    {
        _server
            .Given(Request.Create().WithPath(path).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(status)
                .WithHeader("Content-Type", "application/json")
                .WithBody(body));
    }

    [Test, Category("smoke"), Description("TC-210 happy path from the virtual service")]
    public async Task GetUser_ReturnsStubbedData()
    {
        var response = await _client.GetAsync("/users/1");

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That((string?)response.Json()["name"], Is.EqualTo("Test User"));
        Assert.That((bool)response.Json()["active"]!, Is.True);
    }

    [Test, Description("TC-211 unauthorized is returned with a clear error")]
    public async Task GetUser_Unauthorized()
    {
        var response = await _client.GetAsync("/users/401");

        Assert.That(response.StatusCode, Is.EqualTo(401));
        Assert.That((string?)response.Json()["error"], Is.EqualTo("Unauthorized"));
    }

    [Test, Description("TC-212 server error scenario that is hard to trigger on a real system")]
    public async Task GetUser_ServerError()
    {
        var response = await _client.GetAsync("/users/500");

        Assert.That(response.StatusCode, Is.EqualTo(500));
        Assert.That((string?)response.Json()["error"], Is.EqualTo("Internal Server Error"));
    }

    [Test, Description("TC-213 slow dependency: the call takes at least the simulated delay")]
    public async Task GetUser_SlowService()
    {
        var response = await _client.GetAsync("/users/slow");

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(response.ElapsedMs, Is.GreaterThan(1400));
    }
}
