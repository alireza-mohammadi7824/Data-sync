using Volo.Abp.Modularity;

namespace Monitoring;

[DependsOn(
    typeof(MonitoringDomainSharedModule)
)]
public class MonitoringDomainModule : AbpModule
{
}
