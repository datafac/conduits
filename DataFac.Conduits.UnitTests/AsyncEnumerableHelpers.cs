using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataFac.Conduits.UnitTests;
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

/// <summary>
/// Helpers provided for older versions of .NET because Microsoft hasn't.
/// </summary>
internal static class AsyncEnumerableHelpers
{
    public static async ValueTask<List<T>> ToListAsyncInternal<T>(this IAsyncEnumerable<T> source)
    {
        var result = new List<T>();
        await foreach (var item in source)
        {
            result.Add(item);
        }
        return result;
    }
}
