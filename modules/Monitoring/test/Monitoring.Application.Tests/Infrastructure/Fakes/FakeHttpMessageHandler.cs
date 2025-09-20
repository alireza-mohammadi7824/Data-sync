using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Monitoring.Infrastructure.Fakes;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? _responseFactory;

    public IList<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

    public void SetResponseFactory(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> factory)
    {
        _responseFactory = factory;
    }

    public void Reset()
    {
        Requests.Clear();
        _responseFactory = null;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (_responseFactory is null)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }

        return _responseFactory(request, cancellationToken);
    }
}
