using Monitoring.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Monitoring.Web.Pages;

public abstract class MonitoringPageModel : AbpPageModel
{
    protected MonitoringPageModel()
    {
        LocalizationResourceType = typeof(MonitoringResource);
    }
}
