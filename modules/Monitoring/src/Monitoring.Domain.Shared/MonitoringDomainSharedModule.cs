using Monitoring.Localization;
using Monitoring.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Monitoring;

[DependsOn(
    typeof(AbpAuthorizationModule)
)]
public class MonitoringDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MonitoringDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<MonitoringResource>("en")
                .AddVirtualJson("/Localization/Monitoring");

            options.DefaultResourceType = typeof(MonitoringResource);
        });

        Configure<AbpAuthorizationOptions>(options =>
        {
            options.AddProvider<MonitoringPermissionDefinitionProvider>();
        });
    }
}
