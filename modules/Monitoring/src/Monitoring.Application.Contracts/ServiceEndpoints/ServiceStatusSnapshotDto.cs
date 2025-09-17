using System;
using Monitoring.Enums;
using Volo.Abp.Application.Dtos;

namespace Monitoring.ServiceEndpoints;

public class ServiceStatusSnapshotDto : EntityDto<Guid>
{
    public Guid ServiceEndpointId { get; set; }

    public MonitoringStatus Status { get; set; }

    public DateTime CheckedAt { get; set; }

    public int? DurationMilliseconds { get; set; }

    public string? Notes { get; set; }

    public int? ResultCode { get; set; }
}
