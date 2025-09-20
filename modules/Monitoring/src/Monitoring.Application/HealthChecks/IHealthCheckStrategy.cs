using System.Threading;
using System.Threading.Tasks;
using Monitoring.Enums;

namespace Monitoring.ServiceEndpoints.HealthChecks;

public interface IHealthCheckStrategy
{
    Task<(MonitoringStatus status, int? responseTimeMs, string message)> CheckAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken);
}
