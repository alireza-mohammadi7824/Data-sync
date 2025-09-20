using System;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.ServiceEndpoints.HealthChecks;

namespace Monitoring.Infrastructure.Fakes;

public class FakeTcpConnector : ITcpConnector
{
    private Func<string, int, CancellationToken, Task>? _connectAsync;

    public void SetBehavior(Func<string, int, CancellationToken, Task> behavior)
    {
        _connectAsync = behavior;
    }

    public Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        if (_connectAsync is null)
        {
            return Task.CompletedTask;
        }

        return _connectAsync(host, port, cancellationToken);
    }
}
