using NUnit.Framework;
using Qa.ApiFramework.Api;
using Qa.ApiFramework.Config;
using Qa.ApiFramework.Http;
using Qa.BddTests.Support;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Qa.BddTests.Steps;

/// <summary>
/// Step definitions: each Given/When/Then line in the feature file maps to one method here.
/// The steps stay thin and call the framework (PostsApi, ApiClient), the same way a UI test
/// would call page objects.
/// </summary>
[Binding]
public class PostsSteps
{
    private readonly ApiContext _context;

    public PostsSteps(ApiContext context)
    {
        _context = context;
    }

    [Given(@"the posts API is available")]
    public void GivenThePostsApiIsAvailable()
    {
        _context.BaseUrl = Settings.ApiBaseUrl;
    }

    [Given(@"a virtual service that answers GET (.*) with status (.*)")]
    public void GivenAVirtualServiceThatAnswers(string path, int status)
    {
        _context.Server = WireMockServer.Start();
        _context.Server
            .Given(Request.Create().WithPath(path).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(status)
                .WithHeader("Content-Type", "application/json")
                .WithBody($"{{\"status\":{status}}}"));
        _context.BaseUrl = _context.Server.Urls[0];
    }

    [When(@"I request the post with id (.*)")]
    public async Task WhenIRequestThePostWithId(int id)
    {
        using var api = new PostsApi(_context.BaseUrl);
        _context.Response = await api.GetByIdAsync(id);
    }

    [When(@"I send GET (.*)")]
    public async Task WhenISendGet(string path)
    {
        using var client = new ApiClient(_context.BaseUrl);
        _context.Response = await client.GetAsync(path);
    }

    [Then(@"the response status is (.*)")]
    public void ThenTheResponseStatusIs(int expected)
    {
        Assert.That(_context.Response, Is.Not.Null, "no request was sent");
        Assert.That(_context.Response!.StatusCode, Is.EqualTo(expected));
    }

    [Then(@"the post id in the response is (.*)")]
    public void ThenThePostIdInTheResponseIs(int expected)
    {
        Assert.That((int)_context.Response!.Json()["id"]!, Is.EqualTo(expected));
    }
}
