using HRSDataIntegration;
using Volo.Abp.Modularity;

namespace Monitoring;

public abstract class MonitoringApplicationTestBase<TStartupModule> : HRSDataIntegrationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
}
