using System;
using Monitoring.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Monitoring.ServiceEndpoints;

public class ServiceStatusSnapshot : Entity<Guid>
{
    protected ServiceStatusSnapshot()
    {
    }

    internal ServiceStatusSnapshot(
        Guid id,
        Guid serviceEndpointId,
        MonitoringStatus status,
        DateTime checkedAt,
        TimeSpan? duration,
        string? notes,
        int? resultCode)
        : base(id)
    {
        ServiceEndpointId = serviceEndpointId;
        Status = status;
        CheckedAt = checkedAt;
        DurationMilliseconds = duration.HasValue
            ? ConvertDurationToMilliseconds(duration.Value)
            : null;
        Notes = NormalizeNotes(notes);
        ResultCode = resultCode;
    }

    public Guid ServiceEndpointId { get; private set; }

    public MonitoringStatus Status { get; private set; }

    public DateTime CheckedAt { get; private set; }

    public int? DurationMilliseconds { get; private set; }

    public string? Notes { get; private set; }

    public int? ResultCode { get; private set; }

    private static int ConvertDurationToMilliseconds(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration cannot be negative.");
        }

        var totalMilliseconds = duration.TotalMilliseconds;
        if (totalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration is too large.");
        }

        return (int)Math.Round(totalMilliseconds);
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var value = notes.Trim();
        Check.Length(value, nameof(notes), ServiceStatusSnapshotConsts.MaxNotesLength);
        return value;
    }
}
