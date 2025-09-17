using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(MonitoringDomainSharedModule)
)]
public class MonitoringDomainModule : AbpModule
{
}
