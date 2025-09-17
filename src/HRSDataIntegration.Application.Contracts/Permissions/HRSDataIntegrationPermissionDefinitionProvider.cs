using HRSDataIntegration.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace HRSDataIntegration.Permissions;

public class HRSDataIntegrationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var monitoringGroup = context.AddGroup(HRSDataIntegrationPermissions.GroupName);

        var monitoring = monitoringGroup.AddPermission(
            HRSDataIntegrationPermissions.Monitoring.Default,
            L("Permission:Monitoring"));

        var services = monitoring.AddChild(
            HRSDataIntegrationPermissions.Monitoring.Services.Default,
            L("Permission:MonitoringServices"),
            isEnabledByDefault: true);

        services.AddChild(
            HRSDataIntegrationPermissions.Monitoring.Services.Create,
            L("Permission:MonitoringServicesCreate"),
            isEnabledByDefault: true);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HRSDataIntegrationResource>(name);
    }
}
