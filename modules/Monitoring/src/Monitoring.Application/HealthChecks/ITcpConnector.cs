using System.Threading;
using System.Threading.Tasks;

namespace Monitoring.ServiceEndpoints.HealthChecks;

public interface ITcpConnector
{
    Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default);
}
