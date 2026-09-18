using Nerdbank.MessagePack;
using PolyType;
using Xunit;

namespace DataFac.Conduits.UnitTests;

[GenerateShape]
[DerivedTypeShape(typeof(MessageNode), Tag = 1)]
[DerivedTypeShape(typeof(MessageLeaf), Tag = 2)]
public abstract partial class MessageBase { }

[GenerateShape]
public partial class MessageNode : MessageBase
{
    [Key(1)] public int A { get; set; }
    [Key(2)] public int B { get; set; }
}

[GenerateShape]
public sealed partial class MessageLeaf : MessageNode
{
    [Key(8)] public int X { get; set; }
    [Key(9)] public int Y { get; set; }
}

public partial class MessageLeaf
{
}

public class ShapeTests
{
#if NET8_0_OR_GREATER
    //[Fact]
    //public void ShapeTest1()
    //{
    //    var leaf = new MessageLeaf() { A = 1, B = 2, X = 8, Y = 9 };
    //    ITypeShapeProvider tsp = xxx;
    //    var shape = leaf.GetTypeShape<MessageLeaf>();
    //    //ITypeShapeProvider tsp = leaf.xxx;

    //    IShapeable<MessageLeaf> sss = leaf;

    //    ITypeShape<MessageLeaf> ts = MessageLeaf.GetTypeShape<MessageLeaf>();

    //}
#endif
}