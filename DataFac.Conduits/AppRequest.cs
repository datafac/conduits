using System;

namespace DataFac.Conduits;

public readonly struct AppRequest
{
    public readonly ReadOnlyMemory<byte> Payload;

    public AppRequest(ReadOnlyMemory<byte> payload)
    {
        Payload = payload;
    }
}
