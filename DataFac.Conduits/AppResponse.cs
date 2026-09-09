using System;

namespace DataFac.Conduits;

public readonly struct AppResponse
{
    public readonly ReadOnlyMemory<byte> Payload;

    public AppResponse(ReadOnlyMemory<byte> payload)
    {
        Payload = payload;
    }
}
