using System;

namespace DataFac.Conduits;

public enum ControlCode
{
    Ok = 0, // user payload
    Timeout = 1, // deadline exceeded
}

public readonly struct ConduitRequest
{
    public readonly ControlCode Control;
    public readonly ReadOnlyMemory<byte> Payloadqqq;

    //public ConduitRequest(ReadOnlyMemory<byte> payload)
    //{
    //    Control = ControlCode.None;
    //    Payloadqqq = payload;
    //}
    //public ConduitRequest(ControlCode control)
    //{
    //    Control = control;
    //    Payloadqqq = ReadOnlyMemory<byte>.Empty;
    //}
    public ConduitRequest(ControlCode control, ReadOnlyMemory<byte> payload)
    {
        Control = control;
        Payloadqqq = payload;
    }
}

public readonly struct ConduitResponse
{
    public readonly ControlCode Control;
    public readonly ReadOnlyMemory<byte> Payloadqqq;

    //public ConduitResponse(ReadOnlyMemory<byte> payload)
    //{
    //    Control = ControlCode.None;
    //    Payloadqqq = payload;
    //}
    public ConduitResponse(ControlCode control)
    {
        Control = control;
        Payloadqqq = ReadOnlyMemory<byte>.Empty;
    }
    public ConduitResponse(ControlCode control, ReadOnlyMemory<byte> payload)
    {
        Control = control;
        Payloadqqq = payload;
    }
}
