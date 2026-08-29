using System;

namespace DataFac.Conduits.HttpCommon;

public class JsonPayload
{
    public int Control { get; set; }
    public long? Deadline { get; set; }
    public byte[]? Payload { get; set; }

    public DateTime? GetDeadlineUtc()
    {
        return Deadline.HasValue
            ? new DateTime(Deadline.Value, DateTimeKind.Utc)
            : null;
    }
}