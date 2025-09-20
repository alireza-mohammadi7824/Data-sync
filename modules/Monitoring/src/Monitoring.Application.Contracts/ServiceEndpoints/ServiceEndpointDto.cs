using System;
using System.Collections.Generic;
using Monitoring.Enums;
using Volo.Abp.Application.Dtos;

namespace Monitoring.ServiceEndpoints;

public class ServiceEndpointDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public MonitoringServiceType ServiceType { get; set; }

    public string Target { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public int CheckIntervalSeconds { get; set; }

    public int TimeoutSeconds { get; set; }

    public MonitoringStatus LastKnownStatus { get; set; }

    public DateTime? LastCheckTime { get; set; }

    public int? LastResponseDurationMilliseconds { get; set; }

    public List<ServiceStatusSnapshotDto> StatusSnapshots { get; set; } = new();
}
