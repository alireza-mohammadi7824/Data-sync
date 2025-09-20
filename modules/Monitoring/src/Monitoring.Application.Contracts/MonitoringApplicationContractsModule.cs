using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(MonitoringDomainSharedModule),
    typeof(AbpDddApplicationContractsModule)
)]
public class MonitoringApplicationContractsModule : AbpModule
{
}
