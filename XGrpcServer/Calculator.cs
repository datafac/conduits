using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Testing.Calculator;
using XGrpcShared;

namespace XGrpcServer;

internal class Calculator : IAsyncCalculator
{
    public async ValueTask DisposeAsync() { }

    public async ValueTask<double> DoBinOp(double x, BinOp op, double y)
    {
        return op switch
        {
            BinOp.Add => x + y,
            BinOp.Subtract => x - y,
            BinOp.Multiply => x * y,
            BinOp.Divide => x / y,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }

    public async IAsyncEnumerable<int> GetRange(int start, int count, TimeSpan delay)
    {
        foreach (var i in Enumerable.Range(start, count))
        {
            yield return i;
            await Task.Delay(delay).ConfigureAwait(false);
        }
    }
}
