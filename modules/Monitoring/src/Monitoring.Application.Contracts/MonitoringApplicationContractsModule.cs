using Monitoring.Localization;
using Volo.Abp.Application;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Validation.Localization;

namespace Monitoring;

[DependsOn(
    typeof(MonitoringDomainSharedModule),
    typeof(AbpApplicationContractsModule)
)]
public class MonitoringApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<MonitoringResource>()
                .AddBaseTypes(typeof(AbpValidationResource));
        });
    }
}
