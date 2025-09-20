namespace Monitoring.ServiceEndpoints.HealthChecks;

public interface IProvidesResultCode
{
    int? ResultCode { get; }
}
