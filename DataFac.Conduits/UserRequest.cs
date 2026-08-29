using System;

namespace DataFac.Conduits;

public readonly struct UserRequest
{
    public readonly ReadOnlyMemory<byte> Payload;

    public UserRequest(ReadOnlyMemory<byte> payload)
    {
        Payload = payload;
    }
}
