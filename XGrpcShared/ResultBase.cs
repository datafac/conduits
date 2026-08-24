using PolyType;

namespace XGrpcShared
{
    [GenerateShape]
    [DerivedTypeShape(typeof(ErrorResult), Tag = 1)]
    [DerivedTypeShape(typeof(UnaryResult), Tag = 2)]
    [DerivedTypeShape(typeof(RangeResult), Tag = 3)]
    public abstract partial class ResultBase
    {
    }
}
