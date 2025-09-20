using System;
using Volo.Abp.Timing;

namespace Monitoring.Infrastructure.Fakes;

public class TestClock : IClock
{
    private DateTime _now;

    public TestClock()
    {
        _now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public DateTimeKind Kind => DateTimeKind.Utc;

    public DateTime Now => _now;

    public DateTime Normalize(DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

    public void SetNow(DateTime now)
    {
        _now = DateTime.SpecifyKind(now, DateTimeKind.Utc);
    }
}
