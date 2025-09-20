using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Monitoring.ServiceEndpoints.HealthChecks;

public class DefaultTcpConnector : ITcpConnector
{
    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port, cancellationToken);
    }
}
