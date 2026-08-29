using System;

namespace DataFac.Conduits.HttpClient;

internal static class JsonPayloadExtensions
{
    public static ReadOnlyMemory<byte> ToPayload(this JsonPayload request)
    {
        if (request is null) return default;
        return new ReadOnlyMemory<byte>(request.Body);
    }

    public static JsonPayload ToJsonPayload(this ConduitRequest request, DateTime? deadlineUtc)
    {
        return new JsonPayload()
        {
            Control = (int)request.Control,
            Deadline = deadlineUtc.HasValue ? deadlineUtc.Value.Ticks : null,
            Payload = request.Payloadqqq.ToArray(), // todo alloc
        };
    }
}
