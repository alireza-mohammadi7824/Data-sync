using System.Collections.Generic;
using System.Threading.Tasks;
using HRSDataIntegration.Monitoring;

namespace HRSDataIntegration.Web.Pages.Monitoring;

public class IndexModel : HRSDataIntegrationPageModel
{
    private readonly IMonitoringAppService _monitoringAppService;

    public MonitoringDashboardDto Dashboard { get; private set; } = new();

    public MonitoringSummaryDto Summary => Dashboard?.Summary ?? new MonitoringSummaryDto();

    public IReadOnlyList<JobStatusDto> Jobs => Dashboard?.Jobs ?? new List<JobStatusDto>();

    public IndexModel(IMonitoringAppService monitoringAppService)
    {
        _monitoringAppService = monitoringAppService;
    }

    public async Task OnGetAsync()
    {
        Dashboard = await _monitoringAppService.GetDashboardAsync() ?? new MonitoringDashboardDto();
    }
}
