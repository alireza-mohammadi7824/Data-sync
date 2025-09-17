using System;
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
        context.Services.AddHttpClient("Monitoring", client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        context.Services.AddTransient<ServiceEndpoints.IServiceEndpointAppService, ServiceEndpoints.ServiceEndpointAppService>();
        context.Services.AddTransient<ServiceEndpoints.IHealthCheckAppService, ServiceEndpoints.HealthCheckAppService>();
        context.Services.AddTransient<ServiceEndpoints.HealthChecks.IHealthCheckStrategy, ServiceEndpoints.HealthChecks.HttpHealthCheck>();
        context.Services.AddTransient<ServiceEndpoints.HealthChecks.IHealthCheckStrategy, ServiceEndpoints.HealthChecks.ApiHealthCheck>();
        context.Services.AddTransient<ServiceEndpoints.HealthChecks.IHealthCheckStrategy, ServiceEndpoints.HealthChecks.TcpHealthCheck>();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MonitoringApplicationModule>(validate: true);
        });
    }
}
