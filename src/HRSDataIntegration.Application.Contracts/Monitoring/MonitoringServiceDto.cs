using System;

namespace HRSDataIntegration.Monitoring;

public class MonitoringServiceDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastCheckTime { get; set; }
}
