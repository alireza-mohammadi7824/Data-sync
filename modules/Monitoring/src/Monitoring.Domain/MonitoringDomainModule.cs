using Volo.Abp.Ddd.Domain;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(MonitoringDomainSharedModule),
    typeof(AbpDddDomainModule)
    typeof(AbpDddDomainModule),
    typeof(MonitoringDomainSharedModule)
)]
public class MonitoringDomainModule : AbpModule
{
}
