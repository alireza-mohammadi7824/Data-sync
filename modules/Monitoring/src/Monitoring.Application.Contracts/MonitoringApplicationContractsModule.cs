using Microsoft.Extensions.DependencyInjection;
using Monitoring.Localization;
using Volo.Abp.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(MonitoringDomainSharedModule),
    typeof(AbpDddApplicationContractsModule)
)]
public class MonitoringApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource<MonitoringResource>(typeof(MonitoringApplicationContractsModule).Assembly);
        });
    }
}
