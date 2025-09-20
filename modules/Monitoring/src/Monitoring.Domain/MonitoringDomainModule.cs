using Volo.Abp.Ddd.Domain;
using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(MonitoringDomainSharedModule),
    typeof(AbpDddDomainModule)
)]
public class MonitoringDomainModule : AbpModule
{
}
