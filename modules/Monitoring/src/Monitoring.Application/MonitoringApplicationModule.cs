using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(MonitoringDomainModule),
    typeof(MonitoringApplicationContractsModule)
)]
public class MonitoringApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<MonitoringApplicationModule>();
        context.Services.AddTransient<ServiceEndpoints.ServiceEndpointAppService>();
        context.Services.AddTransient<ServiceEndpoints.HealthCheckAppService>();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MonitoringApplicationModule>(validate: true);
        });
    }
}
