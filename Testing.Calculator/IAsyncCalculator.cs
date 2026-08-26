using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing.Calculator;

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
