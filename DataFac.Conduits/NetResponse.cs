using System;

namespace DataFac.Conduits;

public readonly struct NetResponse
{
    public readonly ControlCode Control;
    public readonly ReadOnlyMemory<byte> Payload;

    public NetResponse(ReadOnlyMemory<byte> payload)
    {
        Control = ControlCode.None;
        Payload = payload;
    }
    public NetResponse(ControlCode control, ReadOnlyMemory<byte> payload)
    {
        Control = control;
        Payload = payload;
    }
}
