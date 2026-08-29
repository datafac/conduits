using System;

namespace DataFac.Conduits;

public readonly struct ConduitRequest
{
    public readonly ControlCode Control;
    public readonly ReadOnlyMemory<byte> Payload;

    public ConduitRequest(ReadOnlyMemory<byte> payload)
    {
        Control = ControlCode.None;
        Payload = payload;
    }
    public ConduitRequest(ControlCode control, ReadOnlyMemory<byte> payload)
    {
        Control = control;
        Payload = payload;
    }
}
