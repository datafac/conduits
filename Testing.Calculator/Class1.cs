using Nerdbank.MessagePack;
using PolyType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing.Calculator;

[GenerateShape]
[DerivedTypeShape(typeof(BinOpRequest), Tag = 1)]
[DerivedTypeShape(typeof(RangeRequest), Tag = 2)]
public abstract partial class RequestBase
{
}

[GenerateShape]
public sealed partial class BinOpRequest : RequestBase
{
    [Key(1)] public double A { get; set; }
    [Key(2)] public double B { get; set; }
    [Key(3)] public BinOp Op { get; set; }
}

[GenerateShape]
public sealed partial class RangeRequest : RequestBase
{
    [Key(1)] public int Start { get; set; }
    [Key(2)] public int Count { get; set; }
    [Key(3)] public TimeSpan Delay { get; set; }
}
[GenerateShape]
[DerivedTypeShape(typeof(ErrorResult), Tag = 1)]
[DerivedTypeShape(typeof(UnaryResult), Tag = 2)]
[DerivedTypeShape(typeof(RangeResult), Tag = 3)]
public abstract partial class ResultBase
{
}
public enum ExcpCode
{
    Undefined = 0,
    UnknownRequest = 1,
    DivideByZero = 2,
    Overflow = 3,
    UnknownOther = 4
}
[GenerateShape]
public sealed partial class UnaryResult : ResultBase
{
    [Key(1)]
    public double X { get; set; }
}
[GenerateShape]
public sealed partial class RangeResult : ResultBase
{
    [Key(1)]
    public int X { get; set; }
}
[GenerateShape]
public sealed partial class ErrorResult : ResultBase
{
    [Key(1)]
    public ExcpCode Code { get; set; }

    [Key(2)]
    public string Message { get; set; } = string.Empty;
}
