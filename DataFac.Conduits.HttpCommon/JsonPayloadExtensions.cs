using System;

namespace DataFac.Conduits.HttpCommon
{
    public static class JsonPayloadExtensions
    {
        public static ReadOnlyMemory<byte> ToPayload(this JsonPayload request)
        {
            if (request is null) return default;
            return new ReadOnlyMemory<byte>(request.Body);
        }

        public static JsonPayload ToUserData(this ReadOnlyMemory<byte> response, long? deadline)
        {
            return new JsonPayload()
            {
                Body = response.ToArray(),
                Deadline = deadline
            };
        }
    }
}