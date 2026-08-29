using System;

namespace DataFac.Conduits;

public readonly struct UserResponse
{
    public readonly ReadOnlyMemory<byte> Payload;

    public UserResponse(ReadOnlyMemory<byte> payload)
    {
        Payload = payload;
    }
}
