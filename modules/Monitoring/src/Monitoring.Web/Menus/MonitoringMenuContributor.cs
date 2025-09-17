using System.Threading.Tasks;
using Monitoring.Localization;
using Monitoring.Permissions;
using Volo.Abp.UI.Navigation;

namespace Monitoring.Menus;

public class MonitoringMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var l = context.GetLocalizer<MonitoringResource>();

        var monitoringMenu = new ApplicationMenuItem(
            name: "Monitoring",
            displayName: l["Menu:Monitoring"],
            url: "/Monitoring",
            icon: "fa fa-heartbeat",
            order: 1000,
            requiredPermissionName: MonitoringPermissions.Default
        );

        if (await context.IsGrantedAsync(MonitoringPermissions.Default))
        {
            context.Menu.AddItem(monitoringMenu);
        }
    }
}
