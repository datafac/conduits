using Nerdbank.MessagePack;
using PolyType;

namespace XGrpcShared
{
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
}
