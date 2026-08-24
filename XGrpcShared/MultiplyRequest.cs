using Nerdbank.MessagePack;
using PolyType;
using System;

namespace XGrpcShared
{
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
        [Key(3)] public TimeSpan Delay{ get; set; }
    }
}
