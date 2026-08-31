using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Calculator;

public class Calculator : IAsyncCalculator
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

    public async IAsyncEnumerable<int> GetRange(int start, int count, TimeSpan delay, [EnumeratorCancellation] CancellationToken cancellation)
    {
        await foreach (var i in AsyncEnumerable.Range(start, count))
        {
            await Task.Delay(delay).ConfigureAwait(false);
            yield return i;
        }
    }
}

