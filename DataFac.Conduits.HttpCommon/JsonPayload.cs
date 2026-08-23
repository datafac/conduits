using System;

namespace DataFac.Conduits.HttpCommon
{
    public class JsonPayload
    {
        public long? Deadline { get; set; }
        public byte[]? Body { get; set; }

        public DateTime? GetDeadlineUtc()
        {
            return Deadline.HasValue
                ? new DateTime(Deadline.Value, DateTimeKind.Utc)
                : null;
        }
    }
}