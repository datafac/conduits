using System;

namespace DataFac.Conduits;

public readonly struct NetRequest
{
    public readonly int Route;
    public readonly ControlCode Control;
    public readonly ReadOnlyMemory<byte> Payload;

    public NetRequest(int route, ControlCode control)
    {
        Route = route;
        Control = control;
        Payload = ReadOnlyMemory<byte>.Empty;
    }
    public NetRequest(int route, ReadOnlyMemory<byte> payload)
    {
        Route = route;
        Control = ControlCode.None;
        Payload = payload;
    }
    public NetRequest(int route, ControlCode control, ReadOnlyMemory<byte> payload)
    {
        Route = route;
        Control = control;
        Payload = payload;
    }
}
