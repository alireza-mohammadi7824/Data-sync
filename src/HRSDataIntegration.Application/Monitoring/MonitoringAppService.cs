using System.Threading.Tasks;

namespace HRSDataIntegration.Monitoring;

public class MonitoringAppService : HRSDataIntegrationAppService, IMonitoringAppService
{
    public Task<MonitoringDashboardDto> GetDashboardAsync()
    {
        var dashboard = new MonitoringDashboardDto
        {
            Summary = new MonitoringSummaryDto
            {
                TotalServices = 0,
                ActiveServices = 0,
                GeneratedAt = default
            }
        };

        return Task.FromResult(dashboard);
    }
}
