using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monitoring.Enums;
using Monitoring.Menus;
using Monitoring.Permissions;
using Monitoring.ServiceEndpoints;

namespace Monitoring.Web.Pages.Monitoring.Services;

[Authorize(MonitoringPermissions.View)]
public class DetailModel : MonitoringPageModel
{
    private readonly IServiceEndpointAppService _serviceEndpointAppService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ServiceEndpointDto Endpoint { get; private set; } = new();

    public DetailModel(IServiceEndpointAppService serviceEndpointAppService)
    {
        _serviceEndpointAppService = serviceEndpointAppService;
    }

    public virtual async Task<IActionResult> OnGetAsync()
    {
        PageLayout.Content.MenuItemName = MonitoringMenus.Monitoring;
        Endpoint = await _serviceEndpointAppService.GetAsync(Id);
        return Page();
    }

    public string GetServiceTypeText(MonitoringServiceType serviceType)
    {
        return L[$"Monitoring:ServiceType.{serviceType}"];
    }

    public string GetStatusText(MonitoringStatus status)
    {
        return L[$"Monitoring:Status.{status}"];
    }

    public string GetStatusBadgeClass(MonitoringStatus status)
    {
        return status switch
        {
            MonitoringStatus.Healthy => "badge bg-success-subtle text-success",
            MonitoringStatus.Degraded => "badge bg-warning-subtle text-warning",
            MonitoringStatus.Unhealthy => "badge bg-danger-subtle text-danger",
            _ => "badge bg-secondary-subtle text-secondary"
        };
    }

    public string GetEnabledText(bool isEnabled)
    {
        return isEnabled ? L["Monitoring:Yes"] : L["Monitoring:No"];
    }

    public string GetDurationText(int seconds)
    {
        return L["Monitoring:SecondsFormat", seconds];
    }

    public string GetResponseDurationText(int? durationMs)
    {
        return durationMs.HasValue
            ? L["Monitoring:MillisecondsFormat", durationMs.Value]
            : L["Monitoring:NotAvailable"];
    }
}
