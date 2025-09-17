using System.ComponentModel.DataAnnotations;

namespace HRSDataIntegration.Monitoring;

public class CreateMonitoringServiceDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1024)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
