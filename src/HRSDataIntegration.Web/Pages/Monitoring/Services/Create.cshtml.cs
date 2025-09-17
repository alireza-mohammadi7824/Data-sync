using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HRSDataIntegration.Monitoring;
using HRSDataIntegration.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSDataIntegration.Web.Pages.Monitoring.Services;

[Authorize(HRSDataIntegrationPermissions.Monitoring.Services.Create)]
public class CreateModel : HRSDataIntegrationPageModel
{
    private readonly IMonitoringAppService _monitoringAppService;

    [BindProperty]
    public CreateServiceViewModel Service { get; set; } = new();

    public CreateModel(IMonitoringAppService monitoringAppService)
    {
        _monitoringAppService = monitoringAppService;
    }

    public Task OnGetAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Service.Name = Service.Name?.Trim();
        Service.Description = string.IsNullOrWhiteSpace(Service.Description)
            ? null
            : Service.Description.Trim();

        await _monitoringAppService.CreateAsync(new CreateMonitoringServiceDto
        {
            Name = Service.Name!,
            Description = Service.Description,
            IsActive = Service.IsActive
        });

        return RedirectToPage("/Monitoring/Index");
    }

    public class CreateServiceViewModel
    {
        [Required]
        [StringLength(128)]
        public string? Name { get; set; }

        [StringLength(1024)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
