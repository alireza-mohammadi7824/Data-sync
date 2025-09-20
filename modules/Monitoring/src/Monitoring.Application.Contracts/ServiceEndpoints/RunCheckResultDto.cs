using System;
using Monitoring.Enums;

namespace Monitoring.ServiceEndpoints;

public class RunCheckResultDto
{
    public Guid ServiceEndpointId { get; set; }

    public MonitoringStatus Status { get; set; }

    public DateTime CheckedAt { get; set; }

    public int? DurationMilliseconds { get; set; }

    public string? Notes { get; set; }

    public int? ResultCode { get; set; }
}
