using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Qa.ApiFramework.Db;
using Qa.ApiFramework.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Qa.ApiTests;

/// <summary>
/// Backend validation pattern: call the API, then use SQL to confirm the stored data matches.
/// An in-memory SQLite database stands in for SQL Server or MySQL so the tests run anywhere.
/// With a real project, only the connection changes.
/// </summary>
[TestFixture]
[Category("virtual")]
[Category("db")]
public class ApiVsDatabaseTests
{
    private WireMockServer _server = null!;
    private ApiClient _client = null!;
    private SqliteConnection _connection = null!;
    private DbValidator _db = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _server = WireMockServer.Start();
        _server
            .Given(Request.Create().WithPath("/payments/TXN1001").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"transactionId\":\"TXN1001\",\"status\":\"COMPLETED\",\"amount\":250.50,\"currency\":\"EUR\"}"));
        _client = new ApiClient(_server.Urls[0]);

        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using (var create = _connection.CreateCommand())
        {
            create.CommandText =
                "CREATE TABLE payments (" +
                "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "transaction_id TEXT, status TEXT, amount REAL, currency TEXT)";
            create.ExecuteNonQuery();
        }

        _db = new DbValidator(_connection);
        const string insert =
            "INSERT INTO payments (transaction_id, status, amount, currency) VALUES (@p0, @p1, @p2, @p3)";
        _db.Execute(insert, "TXN1001", "COMPLETED", 250.50, "EUR");
        _db.Execute(insert, "TXN2002", "PENDING", 40.00, "GBP");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _connection.Dispose();
        _client.Dispose();
        _server.Stop();
        _server.Dispose();
    }

    [Test, Description("TC-230 API response matches the database record")]
    public async Task ApiResponse_MatchesDatabaseRecord()
    {
        var response = await _client.GetAsync("/payments/TXN1001");
        var api = response.Json();

        var row = _db.FindOne(
            "SELECT status, amount, currency FROM payments WHERE transaction_id = @p0",
            (string)api["transactionId"]!);

        Assert.That(response.StatusCode, Is.EqualTo(200));
        Assert.That(row["status"], Is.EqualTo((string)api["status"]!), "status differs between API and DB");
        Assert.That(row["currency"], Is.EqualTo((string)api["currency"]!), "currency differs between API and DB");
        Assert.That(Convert.ToDecimal(row["amount"]), Is.EqualTo((decimal)api["amount"]!), "amount differs between API and DB");
    }

    [Test, Description("TC-231 a duplicate payment is detected with SQL")]
    public void DuplicatePayments_AreDetected()
    {
        const string duplicateSql =
            "SELECT transaction_id, COUNT(*) AS cnt FROM payments GROUP BY transaction_id HAVING COUNT(*) > 1";

        Assert.That(_db.Query(duplicateSql), Is.Empty, "no duplicates expected at the start");

        _db.Execute(
            "INSERT INTO payments (transaction_id, status, amount, currency) VALUES (@p0, @p1, @p2, @p3)",
            "TXN2002", "PENDING", 40.00, "GBP");

        var duplicates = _db.Query(duplicateSql);
        Assert.That(duplicates, Has.Count.EqualTo(1));
        Assert.That(duplicates[0]["transaction_id"], Is.EqualTo("TXN2002"));

        // Clean up so other tests see the original data.
        _db.Execute(
            "DELETE FROM payments WHERE id = (SELECT MAX(id) FROM payments WHERE transaction_id = @p0)",
            "TXN2002");
    }
}
