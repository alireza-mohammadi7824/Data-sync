using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Monitoring.Menus;
using Monitoring.Permissions;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;

namespace Monitoring.Web;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(MonitoringDomainSharedModule)
)]
public class MonitoringWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new MonitoringMenuContributor());
        });

        Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.AuthorizePage("/Monitoring/Index", MonitoringPermissions.Default);
            options.Conventions.AuthorizePage("/Monitoring/Services/Create", MonitoringPermissions.Create);
            options.Conventions.AuthorizePage("/Monitoring/Services/Edit", MonitoringPermissions.Edit);
            options.Conventions.AuthorizePage("/Monitoring/Services/Detail", MonitoringPermissions.View);
        });

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MonitoringWebModule>();
        });

        context.Services.AddMvc().AddApplicationPart(typeof(MonitoringWebModule).Assembly);
    }
}
