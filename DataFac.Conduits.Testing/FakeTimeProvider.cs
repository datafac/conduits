using System;
using System.Threading;

namespace DataFac.Conduits.Testing;

public sealed class FakeTimeProvider : TimeProvider
{
    // todo get impl from platform
    public override long GetTimestamp()
    {
        return base.GetTimestamp();
    }
    public override DateTimeOffset GetUtcNow()
    {
        return base.GetUtcNow();
    }
    public override TimeZoneInfo LocalTimeZone => base.LocalTimeZone;
    public override long TimestampFrequency => base.TimestampFrequency;
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        return base.CreateTimer(callback, state, dueTime, period);
    }
}
