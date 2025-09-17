using System.Collections.Generic;
using System.Threading.Tasks;
using HRSDataIntegration.Monitoring;

namespace HRSDataIntegration.Web.Pages.Monitoring;

public class IndexModel : HRSDataIntegrationPageModel
{
    private readonly IMonitoringAppService _monitoringAppService;

// codex/add-monitoringapplication-to-datasync-gi4p0i
    public MonitoringDashboardDto Dashboard { get; private set; } = new();

    public MonitoringDashboardDto Dashboard { get; private set; }
// main

    public MonitoringSummaryDto Summary => Dashboard?.Summary ?? new MonitoringSummaryDto();

    public IReadOnlyList<JobStatusDto> Jobs => Dashboard?.Jobs ?? new List<JobStatusDto>();

    public IndexModel(IMonitoringAppService monitoringAppService)
    {
        _monitoringAppService = monitoringAppService;
    }

    public async Task OnGetAsync()
    {
// codex/add-monitoringapplication-to-datasync-gi4p0i
        Dashboard = await _monitoringAppService.GetDashboardAsync() ?? new MonitoringDashboardDto();

        Dashboard = await _monitoringAppService.GetDashboardAsync();
// main
    }
}
