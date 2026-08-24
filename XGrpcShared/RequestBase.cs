using PolyType;

namespace XGrpcShared
{
    [GenerateShape]
    [DerivedTypeShape(typeof(BinOpRequest), Tag = 1)]
    [DerivedTypeShape(typeof(RangeRequest), Tag = 2)]
    public abstract partial class RequestBase
    {

    }
}
