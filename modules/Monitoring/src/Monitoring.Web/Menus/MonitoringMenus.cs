namespace Monitoring.Menus;

public static class MonitoringMenus
{
    public const string Prefix = "Monitoring";

    public const string Monitoring = Prefix;

    public static class Services
    {
        public const string Group = Prefix + ".Services";

        public const string Create = Group + ".Create";
        public const string Edit = Group + ".Edit";
        public const string Detail = Group + ".Detail";
    }
}
