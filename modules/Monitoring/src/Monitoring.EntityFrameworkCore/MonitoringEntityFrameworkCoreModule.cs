using Microsoft.Extensions.DependencyInjection;
using Monitoring.EntityFrameworkCore.ServiceEndpoints;
using Monitoring.ServiceEndpoints;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Monitoring.EntityFrameworkCore;

[DependsOn(
    typeof(MonitoringDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class MonitoringEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<MonitoringDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<ServiceEndpoint, ServiceEndpointRepository>();
        });
    }
}
