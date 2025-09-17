using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Monitoring.Enums;
using Monitoring.Menus;
using Monitoring.Permissions;
using Monitoring.ServiceEndpoints;

namespace Monitoring.Web.Pages.Monitoring.Services;

[Authorize(MonitoringPermissions.Edit)]
public class EditModel : MonitoringPageModel
{
    private readonly IServiceEndpointAppService _serviceEndpointAppService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateServiceEndpointDto Service { get; set; } = new();

    public string EndpointName { get; private set; } = string.Empty;

    public IReadOnlyList<SelectListItem> ServiceTypeOptions { get; private set; } = Array.Empty<SelectListItem>();

    public EditModel(IServiceEndpointAppService serviceEndpointAppService)
    {
        _serviceEndpointAppService = serviceEndpointAppService;
    }

    public virtual async Task<IActionResult> OnGetAsync()
    {
        PageLayout.Content.MenuItemName = MonitoringMenus.Monitoring;
        ServiceTypeOptions = BuildServiceTypeOptions();

        var endpoint = await _serviceEndpointAppService.GetAsync(Id);
        EndpointName = endpoint.Name;

        Service = new CreateUpdateServiceEndpointDto
        {
            Name = endpoint.Name,
            Description = endpoint.Description,
            ServiceType = endpoint.ServiceType,
            Target = endpoint.Target,
            IsEnabled = endpoint.IsEnabled,
            CheckIntervalSeconds = endpoint.CheckIntervalSeconds,
            TimeoutSeconds = endpoint.TimeoutSeconds
        };

        return Page();
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        PageLayout.Content.MenuItemName = MonitoringMenus.Monitoring;
        ServiceTypeOptions = BuildServiceTypeOptions();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _serviceEndpointAppService.UpdateAsync(Id, Service);

        return RedirectToPage("/Monitoring/Services/Detail", new { Id });
    }

    private IReadOnlyList<SelectListItem> BuildServiceTypeOptions()
    {
        return Enum.GetValues<MonitoringServiceType>()
            .Select(type => new SelectListItem
            {
                Text = L[$"Monitoring:ServiceType.{type}"],
                Value = type.ToString()
            })
            .ToList();
    }
}
