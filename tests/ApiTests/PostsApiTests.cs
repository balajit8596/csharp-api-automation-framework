using NUnit.Framework;
using Qa.ApiFramework.Api;
using Qa.ApiFramework.Config;
using Qa.ApiFramework.Data;
using Qa.ApiFramework.Models;
using Qa.ApiFramework.Validation;

namespace Qa.ApiTests;

/// <summary>
/// REST tests against a live public API (JSONPlaceholder). Needs internet access.
/// Run only these with: dotnet test --filter "Category=live"
/// </summary>
[TestFixture]
[Category("live")]
public class PostsApiTests
{
    private PostsApi _api = null!;

    [OneTimeSetUp]
    public void CreateClient() => _api = new PostsApi(Settings.ApiBaseUrl);

    [OneTimeTearDown]
    public void DisposeClient() => _api.Dispose();

    [Test, Category("smoke"), Description("TC-200 list returns 200 and data")]
    public async Task GetAllPosts_ReturnsList()
    {
        var response = await _api.GetAllAsync();

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(response.JsonArray().Count, Is.GreaterThan(0));
    }

    [Test, Category("smoke"), Description("TC-201 single post matches the JSON schema")]
    public async Task GetPost_MatchesSchema()
    {
        var response = await _api.GetByIdAsync(1);
        var schema = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Schemas", "post-schema.json"));

        var errors = await SchemaValidator.ValidateAsync(schema, response.Content);

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(errors, Is.Empty, string.Join("; ", errors));
        Assert.That((int)response.Json()["id"]!, Is.EqualTo(1));
    }

    [Test, Description("TC-202 create returns 201 and echoes the data")]
    public async Task CreatePost_Returns201()
    {
        var request = TestDataFactory.NewPost();

        var response = await _api.CreateAsync(request);
        var created = response.As<Post>();

        Assert.That(response.StatusCode, Is.EqualTo(201));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Title, Is.EqualTo(request.Title));
        Assert.That(created.UserId, Is.EqualTo(request.UserId));
        Assert.That(created.Id, Is.Not.Null);
    }

    [Test, Description("TC-203 update returns 200 with the new data")]
    public async Task UpdatePost_Returns200()
    {
        var update = TestDataFactory.NewPost();
        update.Id = 1;
        update.Title = "updated title";

        var response = await _api.UpdateAsync(1, update);

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That((string?)response.Json()["title"], Is.EqualTo("updated title"));
    }

    [Test, Description("TC-204 delete returns 200")]
    public async Task DeletePost_Returns200()
    {
        var response = await _api.DeleteAsync(1);

        Assert.That(response.StatusCode, Is.EqualTo(200));
    }

    [Test, Description("TC-205 unknown resource returns 404")]
    public async Task GetUnknownPost_Returns404()
    {
        var response = await _api.GetByIdAsync(999999);

        Assert.That(response.StatusCode, Is.EqualTo(404));
    }

    [Test, Description("TC-206 filter by query parameter")]
    public async Task GetPostsByUser_OnlyReturnsThatUser()
    {
        var response = await _api.GetByUserAsync(1);

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(response.JsonArray().All(post => (int)post["userId"]! == 1), Is.True);
    }

    [Test, Description("TC-207 response time is within the limit")]
    public async Task GetPost_RespondsQuickly()
    {
        var response = await _api.GetByIdAsync(1);

        Assert.That(response.ElapsedMs, Is.LessThan(Settings.MaxResponseMs));
    }

    [Test, Description("TC-208 chain: use data from one call in the next")]
    public async Task ChainedCalls_SecondCallUsesFirstResponse()
    {
        var first = await _api.GetByUserAsync(1);
        var firstId = (int)first.JsonArray()[0]["id"]!;

        var second = await _api.GetByIdAsync(firstId);

        Assert.That(second.StatusCode, Is.EqualTo(200));
        Assert.That((int)second.Json()["id"]!, Is.EqualTo(firstId));
    }

    [Test, Description("TC-209 data driven: several ids")]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public async Task GetPost_DataDriven(int id)
    {
        var response = await _api.GetByIdAsync(id);

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That((int)response.Json()["id"]!, Is.EqualTo(id));
    }
}
