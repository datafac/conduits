using System;

namespace DataFac.Conduits;

public readonly struct NetRequest
{
    public readonly ControlCode Control;
    public readonly ReadOnlyMemory<byte> Payload;

    public NetRequest(ReadOnlyMemory<byte> payload)
    {
        Control = ControlCode.None;
        Payload = payload;
    }
    public NetRequest(ControlCode control, ReadOnlyMemory<byte> payload)
    {
        Control = control;
        Payload = payload;
    }
}
