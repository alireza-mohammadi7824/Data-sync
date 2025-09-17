namespace HRSDataIntegration.Permissions;

public static class HRSDataIntegrationPermissions
{
    public const string GroupName = "HRSDataIntegration";

    public static class Monitoring
    {
        public const string Default = GroupName + ".Monitoring";

        public static class Services
        {
            public const string Default = Monitoring.Default + ".Services";
            public const string Create = Default + ".Create";
        }
    }
}
