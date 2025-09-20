using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Monitoring.BackgroundWorkers;
using Monitoring.Options;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(AbpApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(MonitoringDomainModule),
    typeof(MonitoringApplicationContractsModule)
)]
public class MonitoringApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<MonitoringApplicationModule>();
        context.Services.AddOptions<MonitoringOptions>().BindConfiguration("Monitoring");
        context.Services.AddHttpClient("Monitoring", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        context.Services.AddTransient<ServiceEndpoints.IServiceEndpointAppService, ServiceEndpoints.ServiceEndpointAppService>();
        context.Services.AddTransient<ServiceEndpoints.IHealthCheckAppService, ServiceEndpoints.HealthCheckAppService>();
        context.Services.AddTransient<ServiceEndpoints.HealthChecks.ITcpConnector, ServiceEndpoints.HealthChecks.DefaultTcpConnector>();
        context.Services.AddTransient<ServiceEndpoints.HealthChecks.IHealthCheckStrategy, ServiceEndpoints.HealthChecks.HttpHealthCheck>();
        context.Services.AddTransient<ServiceEndpoints.HealthChecks.IHealthCheckStrategy, ServiceEndpoints.HealthChecks.ApiHealthCheck>();
        context.Services.AddTransient<ServiceEndpoints.HealthChecks.IHealthCheckStrategy, ServiceEndpoints.HealthChecks.TcpHealthCheck>();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<MonitoringApplicationModule>(validate: true);
        });
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<MonitoringBackgroundWorker>();
    }
}
