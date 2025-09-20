using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Monitoring.Enums;
using Monitoring.Menus;
using Monitoring.Permissions;
using Monitoring.ServiceEndpoints;
using Volo.Abp.Application.Dtos;

namespace Monitoring.Web.Pages.Monitoring;

[Authorize(MonitoringPermissions.Default)]
public class IndexModel : MonitoringPageModel
{
    private readonly IServiceEndpointAppService _serviceEndpointAppService;

    public IReadOnlyList<ServiceEndpointDto> Endpoints { get; private set; } = Array.Empty<ServiceEndpointDto>();

    public IndexModel(IServiceEndpointAppService serviceEndpointAppService)
    {
        _serviceEndpointAppService = serviceEndpointAppService;
    }

    public virtual async Task OnGetAsync()
    {
        PageLayout.Content.MenuItemName = MonitoringMenus.Monitoring;

        var result = await _serviceEndpointAppService.GetListAsync(
            new PagedAndSortedResultRequestDto
            {
                MaxResultCount = LimitedResultRequestDto.MaxMaxResultCount,
                Sorting = nameof(ServiceEndpointDto.Name)
            });

        Endpoints = result.Items;
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
            MonitoringStatus.Healthy => "bg-success-subtle text-success",
            MonitoringStatus.Degraded => "bg-warning-subtle text-warning",
            MonitoringStatus.Unhealthy => "bg-danger-subtle text-danger",
            _ => "bg-secondary-subtle text-secondary"
        };
    }

    public string GetEnabledText(bool isEnabled)
    {
        return isEnabled ? L["Monitoring:Yes"] : L["Monitoring:No"];
    }
}
