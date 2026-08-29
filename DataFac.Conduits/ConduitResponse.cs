using System;

namespace DataFac.Conduits;

public readonly struct ConduitResponse
{
    public readonly ControlCode Control;
    public readonly ReadOnlyMemory<byte> Payload;

    public ConduitResponse(ReadOnlyMemory<byte> payload)
    {
        Control = ControlCode.None;
        Payload = payload;
    }
    public ConduitResponse(ControlCode control, ReadOnlyMemory<byte> payload)
    {
        Control = control;
        Payload = payload;
    }
}
