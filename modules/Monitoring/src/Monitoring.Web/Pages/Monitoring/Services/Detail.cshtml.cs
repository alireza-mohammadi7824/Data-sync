using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly IHealthCheckAppService _healthCheckAppService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ServiceEndpointDto Endpoint { get; private set; } = new();

    public List<ServiceStatusSnapshotDto> History { get; private set; } = new();

    public Dictionary<string, string> StatusTextLookup { get; private set; } = new();

    public Dictionary<string, string> StatusBadgeLookup { get; private set; } = new();

    public DetailModel(
        IServiceEndpointAppService serviceEndpointAppService,
        IHealthCheckAppService healthCheckAppService)
    {
        _serviceEndpointAppService = serviceEndpointAppService;
        _healthCheckAppService = healthCheckAppService;
    }

    public virtual async Task<IActionResult> OnGetAsync()
    {
        PageLayout.Content.MenuItemName = MonitoringMenus.Monitoring;
        Endpoint = await _serviceEndpointAppService.GetAsync(Id);
        History = await _healthCheckAppService.GetHistoryAsync(Id, 50);
        PopulateLookups();
        return Page();
    }

    [Authorize(MonitoringPermissions.RunCheck)]
    public virtual async Task<JsonResult> OnPostRunCheckAsync()
    {
        var result = await _healthCheckAppService.RunCheckAsync(Id);
        var history = await _healthCheckAppService.GetHistoryAsync(Id, 50);

        return new JsonResult(new
        {
            result,
            history
        });
    }

    [Authorize(MonitoringPermissions.Delete)]
    public virtual async Task<IActionResult> OnPostDeleteAsync()
    {
        await _serviceEndpointAppService.DeleteAsync(Id);

        return NoContent();
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

    private void PopulateLookups()
    {
        StatusTextLookup = Enum.GetValues<MonitoringStatus>()
            .ToDictionary(status => ((int)status).ToString(), GetStatusText);

        StatusBadgeLookup = Enum.GetValues<MonitoringStatus>()
            .ToDictionary(status => ((int)status).ToString(), GetStatusBadgeClass);
    }
}
