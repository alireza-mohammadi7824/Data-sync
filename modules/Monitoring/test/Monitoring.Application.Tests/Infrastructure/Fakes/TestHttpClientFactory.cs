using System.Net.Http;

namespace Monitoring.Infrastructure.Fakes;

public class TestHttpClientFactory : IHttpClientFactory
{
    private readonly FakeHttpMessageHandler _handler;

    public TestHttpClientFactory(FakeHttpMessageHandler handler)
    {
        _handler = handler;
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(_handler, disposeHandler: false);
    }
}
