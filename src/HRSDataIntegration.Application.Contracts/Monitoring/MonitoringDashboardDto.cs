using System.Collections.Generic;

namespace HRSDataIntegration.Monitoring;

public class MonitoringDashboardDto
{
    public MonitoringSummaryDto Summary { get; set; } = new();

    public List<MonitoringServiceDto> Services { get; set; } = new();
}
