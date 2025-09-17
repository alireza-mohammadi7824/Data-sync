using Monitoring.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Monitoring.Permissions;

public class MonitoringPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var monitoringGroup = context.AddGroup(MonitoringPermissions.GroupName, L("Permission:Monitoring"));

        var monitoringPermission = monitoringGroup.AddPermission(MonitoringPermissions.Default, L("Permission:Monitoring.Default"));
        monitoringPermission.AddChild(MonitoringPermissions.View, L("Permission:Monitoring.View"));
        monitoringPermission.AddChild(MonitoringPermissions.Create, L("Permission:Monitoring.Create"));
        monitoringPermission.AddChild(MonitoringPermissions.Edit, L("Permission:Monitoring.Edit"));
        monitoringPermission.AddChild(MonitoringPermissions.Delete, L("Permission:Monitoring.Delete"));
        monitoringPermission.AddChild(MonitoringPermissions.RunCheck, L("Permission:Monitoring.RunCheck"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MonitoringResource>(name);
    }
}
