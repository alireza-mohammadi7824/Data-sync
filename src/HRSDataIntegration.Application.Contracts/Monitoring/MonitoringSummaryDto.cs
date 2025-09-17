using System;

// codex/add-monitoringapplication-to-datasync-siqgy5
namespace HRSDataIntegration.Monitoring;

public class MonitoringSummaryDto
{
    public int TotalJobs { get; set; }

    public int ActiveJobs { get; set; }

    public int TotalJobDetails { get; set; }

    public int TotalJobGroups { get; set; }

    public int TotalJobRasteh { get; set; }

    public DateTime GeneratedAt { get; set; }

namespace HRSDataIntegration.Monitoring
{
    public class MonitoringSummaryDto
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalJobDetails { get; set; }

        // from codex/add-monitoringapplication-to-datasync-gi4p0i
        public int TotalJobGroups { get; set; }
        public int TotalJobRasteh { get; set; }

        // from main
        public int TotalUnits { get; set; }
        public int TotalPosts { get; set; }

        public DateTime GeneratedAt { get; set; }
    }
// main
}
