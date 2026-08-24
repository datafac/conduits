using Nerdbank.MessagePack;
using PolyType;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Testing.Calculator
{
    public enum BinOp
    {
        None = 0,
        Add = 1,
        Subtract = 2,
        Multiply = 3,
        Divide = 4,
    }

    public interface IAsyncCalculator : IAsyncDisposable
    {
        /// <summary>
        /// Returns the binary operation (x op y) of two doubles.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="op"></param>
        /// <param name="y"></param>
        ValueTask<double> DoBinOp(double x, BinOp op, double y);

        /// <summary>
        /// Returns a stream of integers starting at <paramref name="start"/> and continuing 
        /// for <paramref name="count"/> integers, with a delay of <paramref name="delay"/> 
        /// between each integer.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        IAsyncEnumerable<int> GetRange(int start, int count, TimeSpan delay);
    }

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
}
