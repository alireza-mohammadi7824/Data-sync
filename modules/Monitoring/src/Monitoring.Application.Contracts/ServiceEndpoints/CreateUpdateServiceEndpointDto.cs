using Monitoring.Enums;
using System.ComponentModel.DataAnnotations;

namespace Monitoring.ServiceEndpoints;

public class CreateUpdateServiceEndpointDto
{
    [Display(Name = "Monitoring:Fields.Name")]
    [Required(ErrorMessage = "Monitoring:Validation.Name.Required")]
    [StringLength(ServiceEndpointConsts.MaxNameLength, ErrorMessage = "Monitoring:Validation.Name.Length")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Monitoring:Fields.Description")]
    [StringLength(ServiceEndpointConsts.MaxDescriptionLength, ErrorMessage = "Monitoring:Validation.Description.Length")]
    public string? Description { get; set; }

    [Display(Name = "Monitoring:Fields.ServiceType")]
    [Required(ErrorMessage = "Monitoring:Validation.ServiceType.Required")]
    public MonitoringServiceType ServiceType { get; set; }

    [Display(Name = "Monitoring:Fields.Target")]
    [Required(ErrorMessage = "Monitoring:Validation.Target.Required")]
    [StringLength(ServiceEndpointConsts.MaxTargetLength, ErrorMessage = "Monitoring:Validation.Target.Length")]
    public string Target { get; set; } = string.Empty;

    [Display(Name = "Monitoring:Fields.IsEnabled")]
    public bool IsEnabled { get; set; } = true;

    [Display(Name = "Monitoring:Fields.CheckIntervalSeconds")]
    [Range(ServiceEndpointConsts.MinCheckIntervalSeconds, int.MaxValue, ErrorMessage = "Monitoring:Validation.CheckIntervalSeconds.Range")]
    public int CheckIntervalSeconds { get; set; }

    [Display(Name = "Monitoring:Fields.TimeoutSeconds")]
    [Range(ServiceEndpointConsts.MinTimeoutSeconds, int.MaxValue, ErrorMessage = "Monitoring:Validation.TimeoutSeconds.Range")]
    public int TimeoutSeconds { get; set; }
}
