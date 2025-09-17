using Monitoring.Enums;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.ServiceEndpoints;

public class CreateUpdateServiceEndpointDto
{
    [Required]
    [StringLength(ServiceEndpointConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(ServiceEndpointConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [Required]
    public MonitoringServiceType ServiceType { get; set; }

    [Required]
    [StringLength(ServiceEndpointConsts.MaxTargetLength)]
    public string Target { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    [Range(ServiceEndpointConsts.MinCheckIntervalSeconds, int.MaxValue)]
    public int CheckIntervalSeconds { get; set; }

    [Range(ServiceEndpointConsts.MinTimeoutSeconds, int.MaxValue)]
    public int TimeoutSeconds { get; set; }
}
