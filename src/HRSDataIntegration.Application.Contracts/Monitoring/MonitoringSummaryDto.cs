using System;

namespace HRSDataIntegration.Monitoring;

public class MonitoringSummaryDto
{
    public int TotalServices { get; set; }

    public int ActiveServices { get; set; }

    public DateTime GeneratedAt { get; set; }
}
