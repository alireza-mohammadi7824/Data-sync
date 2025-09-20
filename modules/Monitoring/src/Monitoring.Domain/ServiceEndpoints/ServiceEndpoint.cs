using System;
using System.Collections.Generic;
using Monitoring.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Monitoring.ServiceEndpoints;

public class ServiceEndpoint : FullAuditedAggregateRoot<Guid>
{
    private readonly List<ServiceStatusSnapshot> _statusSnapshots = new();

    protected ServiceEndpoint()
    {
    }

    public ServiceEndpoint(
        Guid id,
        string name,
        MonitoringServiceType serviceType,
        string target,
        TimeSpan checkInterval,
        TimeSpan timeout,
        bool isEnabled = true,
        string? description = null)
        : base(id)
    {
        SetName(name);
        ServiceType = serviceType;
        SetTarget(target);
        SetDescription(description);
        SetSchedule(checkInterval, timeout);
        IsEnabled = isEnabled;
        LastKnownStatus = MonitoringStatus.Unknown;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public MonitoringServiceType ServiceType { get; private set; }

    public string Target { get; private set; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public int CheckIntervalSeconds { get; private set; }

    public TimeSpan CheckInterval => TimeSpan.FromSeconds(CheckIntervalSeconds);

    public int TimeoutSeconds { get; private set; }

    public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutSeconds);

    public MonitoringStatus LastKnownStatus { get; private set; }

    public DateTime? LastCheckTime { get; private set; }

    public int? LastResponseDurationMilliseconds { get; private set; }

    public IReadOnlyCollection<ServiceStatusSnapshot> StatusSnapshots => _statusSnapshots.AsReadOnly();

    public void Update(
        string name,
        MonitoringServiceType serviceType,
        string target,
        TimeSpan checkInterval,
        TimeSpan timeout,
        bool isEnabled,
        string? description = null)
    {
        SetName(name);
        ServiceType = serviceType;
        SetTarget(target);
        SetDescription(description);
        SetSchedule(checkInterval, timeout);
        IsEnabled = isEnabled;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    public ServiceStatusSnapshot RecordSnapshot(
        Guid snapshotId,
        MonitoringStatus status,
        DateTime checkedAt,
        TimeSpan? duration = null,
        string? notes = null,
        int? resultCode = null)
    {
        var snapshot = new ServiceStatusSnapshot(
            snapshotId,
            Id,
            status,
            checkedAt,
            duration,
            notes,
            resultCode);

        _statusSnapshots.Add(snapshot);

        LastKnownStatus = status;
        LastCheckTime = checkedAt;
        LastResponseDurationMilliseconds = snapshot.DurationMilliseconds;

        return snapshot;
    }

    public void ClearHistory()
    {
        _statusSnapshots.Clear();
    }

    private void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        Check.Length(Name, nameof(name), ServiceEndpointConsts.MaxNameLength);
    }

    private void SetTarget(string target)
    {
        Target = Check.NotNullOrWhiteSpace(target, nameof(target));
        Check.Length(Target, nameof(target), ServiceEndpointConsts.MaxTargetLength);
    }

    private void SetDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
            return;
        }

        Description = description.Trim();
        Check.Length(Description, nameof(description), ServiceEndpointConsts.MaxDescriptionLength);
    }

    private void SetSchedule(TimeSpan checkInterval, TimeSpan timeout)
    {
        if (checkInterval < TimeSpan.FromSeconds(ServiceEndpointConsts.MinCheckIntervalSeconds))
        {
            throw new ArgumentOutOfRangeException(
                nameof(checkInterval),
                checkInterval,
                $"Check interval must be at least {ServiceEndpointConsts.MinCheckIntervalSeconds} seconds.");
        }

        if (timeout < TimeSpan.FromSeconds(ServiceEndpointConsts.MinTimeoutSeconds))
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                timeout,
                $"Timeout must be at least {ServiceEndpointConsts.MinTimeoutSeconds} second.");
        }

        CheckIntervalSeconds = (int)Math.Round(checkInterval.TotalSeconds);
        TimeoutSeconds = (int)Math.Round(timeout.TotalSeconds);
    }
}
