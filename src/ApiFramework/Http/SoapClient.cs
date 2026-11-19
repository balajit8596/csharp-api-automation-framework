using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace Qa.ApiFramework.Http;

/// <summary>The result of a SOAP call, with a helper to read values from the XML.</summary>
public sealed class SoapResult
{
    public SoapResult(int statusCode, string body, long elapsedMs)
    {
        StatusCode = statusCode;
        Body = body;
        ElapsedMs = elapsedMs;
    }

    public int StatusCode { get; }

    public string Body { get; }

    public long ElapsedMs { get; }

    public XDocument Xml => XDocument.Parse(Body);

    /// <summary>First element with this local name (namespace and prefix ignored), or null.</summary>
    public string? ValueOf(string localName)
        => Xml.Descendants().FirstOrDefault(e => e.Name.LocalName == localName)?.Value;
}

/// <summary>
/// A SOAP call is an HTTP POST with an XML envelope and a SOAPAction header.
/// </summary>
public sealed class SoapClient : IDisposable
{
    private readonly HttpClient _http = new();

    public async Task<SoapResult> SendAsync(string endpointUrl, string soapAction, string envelope)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
        {
            Content = new StringContent(envelope, Encoding.UTF8, "text/xml")
        };
        request.Headers.TryAddWithoutValidation("SOAPAction", $"\"{soapAction}\"");

        var timer = Stopwatch.StartNew();
        using var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        timer.Stop();

        return new SoapResult((int)response.StatusCode, body, timer.ElapsedMilliseconds);
    }

    public void Dispose() => _http.Dispose();
}
