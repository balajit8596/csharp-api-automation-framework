using NUnit.Framework;
using Qa.ApiFramework.Http;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Qa.ApiTests;

/// <summary>
/// SOAP testing: an HTTP POST with an XML envelope and a SOAPAction header. The service is
/// virtualized, so the tests run offline. The matching WSDL is in parasoft/soap/calculator.wsdl,
/// so the same service can be imported into Parasoft SOAtest.
/// </summary>
[TestFixture]
[Category("virtual")]
public class SoapServiceTests
{
    private const string Path = "/soap/calculator";

    private const string AddRequest = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <Add xmlns="http://tempuri.org/">
              <a>2</a>
              <b>3</b>
            </Add>
          </soap:Body>
        </soap:Envelope>
        """;

    private const string DivideByZeroRequest = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <Divide xmlns="http://tempuri.org/">
              <a>10</a>
              <b>0</b>
            </Divide>
          </soap:Body>
        </soap:Envelope>
        """;

    private WireMockServer _server = null!;
    private SoapClient _soap = null!;

    [OneTimeSetUp]
    public void StartVirtualSoapService()
    {
        _server = WireMockServer.Start();

        _server
            .Given(Request.Create().WithPath(Path).UsingPost().WithHeader("SOAPAction", "*Add*"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/xml; charset=utf-8")
                .WithBody("""
                    <Envelope xmlns="http://schemas.xmlsoap.org/soap/envelope/">
                      <Body>
                        <AddResponse xmlns="http://tempuri.org/">
                          <AddResult>5</AddResult>
                        </AddResponse>
                      </Body>
                    </Envelope>
                    """));

        _server
            .Given(Request.Create().WithPath(Path).UsingPost().WithHeader("SOAPAction", "*Divide*"))
            .RespondWith(Response.Create()
                .WithStatusCode(500)
                .WithHeader("Content-Type", "text/xml; charset=utf-8")
                .WithBody("""
                    <Envelope xmlns="http://schemas.xmlsoap.org/soap/envelope/">
                      <Body>
                        <Fault>
                          <faultcode>Server</faultcode>
                          <faultstring>Division by zero</faultstring>
                        </Fault>
                      </Body>
                    </Envelope>
                    """));

        _soap = new SoapClient();
    }

    [OneTimeTearDown]
    public void StopVirtualSoapService()
    {
        _soap.Dispose();
        _server.Stop();
        _server.Dispose();
    }

    [Test, Category("smoke"), Description("TC-220 SOAP Add returns the correct result")]
    public async Task SoapAdd_ReturnsSum()
    {
        var result = await _soap.SendAsync(_server.Urls[0] + Path, "http://tempuri.org/Add", AddRequest);

        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.ValueOf("AddResult"), Is.EqualTo("5"));
    }

    [Test, Description("TC-221 SOAP fault is returned for invalid input")]
    public async Task SoapDivideByZero_ReturnsFault()
    {
        var result = await _soap.SendAsync(_server.Urls[0] + Path, "http://tempuri.org/Divide", DivideByZeroRequest);

        Assert.That(result.StatusCode, Is.EqualTo(500));
        Assert.That(result.ValueOf("faultstring"), Is.EqualTo("Division by zero"));
    }
}
