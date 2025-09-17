using System.Collections.Generic;
using System.Threading.Tasks;
using HRSDataIntegration.Monitoring;

namespace HRSDataIntegration.Web.Pages.Monitoring;

public class IndexModel : HRSDataIntegrationPageModel
{
    private readonly IMonitoringAppService _monitoringAppService;

    public MonitoringDashboardDto MonitoringDashboard { get; private set; } = new();

    public MonitoringSummaryDto MonitoringSummary => MonitoringDashboard?.Summary ?? new MonitoringSummaryDto();

    public IReadOnlyList<JobStatusDto> MonitoringJobs => MonitoringDashboard?.Jobs ?? new List<JobStatusDto>();

    public IndexModel(IMonitoringAppService monitoringAppService)
    {
        _monitoringAppService = monitoringAppService;
    }

    public async Task OnGetAsync()
    {
        MonitoringDashboard = await _monitoringAppService.GetDashboardAsync() ?? new MonitoringDashboardDto();
    }
}
