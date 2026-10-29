using DataFac.Conduits;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Calculator;

public interface IAsyncCalculator : IAsyncDisposable
{
    /// <summary>
    /// Returns app info.
    /// </summary>
    ValueTask<AppInfo> GetAppInfo(CancellationToken cancellation = default);

    /// <summary>
    /// Returns the binary operation (x op y) of two doubles.
    /// </summary>
    ValueTask<double> DoBinOp(double x, BinOp op, double y, CancellationToken cancellation = default);

    /// <summary>
    /// Returns a stream of integers starting at <paramref name="start"/> and continuing 
    /// for <paramref name="count"/> integers, with a delay of <paramref name="delay"/> 
    /// between each integer.
    /// </summary>
    IAsyncEnumerable<int> GetRange(int start, int count, TimeSpan delay, CancellationToken cancellation = default);
}
