using System;

namespace HRSDataIntegration.Monitoring;

public class MonitoringSummaryDto
{
    public int TotalJobs { get; set; }

    public int ActiveJobs { get; set; }

    public int TotalJobDetails { get; set; }

    public int TotalJobGroups { get; set; }

    public int TotalJobRasteh { get; set; }

    public DateTime GeneratedAt { get; set; }
}
