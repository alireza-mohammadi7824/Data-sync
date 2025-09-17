using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace HRSDataIntegration.Monitoring;

public interface IMonitoringAppService : IApplicationService
{
    Task<MonitoringDashboardDto> GetDashboardAsync();
}
