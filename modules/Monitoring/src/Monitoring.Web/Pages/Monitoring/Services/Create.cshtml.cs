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

[Authorize(MonitoringPermissions.Create)]
public class CreateModel : MonitoringPageModel
{
    private readonly IServiceEndpointAppService _serviceEndpointAppService;

    [BindProperty]
    public CreateUpdateServiceEndpointDto Service { get; set; } = new();

    public IReadOnlyList<SelectListItem> ServiceTypeOptions { get; private set; } = Array.Empty<SelectListItem>();

    public CreateModel(IServiceEndpointAppService serviceEndpointAppService)
    {
        _serviceEndpointAppService = serviceEndpointAppService;
    }

    public virtual Task OnGetAsync()
    {
        PageLayout.Content.MenuItemName = MonitoringMenus.Monitoring;
        Service.ServiceType = MonitoringServiceType.Http;
        Service.CheckIntervalSeconds = 60;
        Service.TimeoutSeconds = 30;
        ServiceTypeOptions = BuildServiceTypeOptions();
        return Task.CompletedTask;
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        PageLayout.Content.MenuItemName = MonitoringMenus.Monitoring;
        ServiceTypeOptions = BuildServiceTypeOptions();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _serviceEndpointAppService.CreateAsync(Service);

        await Notify.SuccessAsync(L["Monitoring:ToastCreateSuccess", Service.Name]);

        return RedirectToPage("/Monitoring/Index");
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
