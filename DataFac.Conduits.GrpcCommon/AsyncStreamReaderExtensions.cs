using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DataFac.Conduits.GrpcCommon;

public static class AsyncStreamReaderExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IAsyncStreamReader<T> stream,
        [EnumeratorCancellation] CancellationToken token)
    {
        while (await stream.MoveNext(token))
        {
            yield return stream.Current;
        }
    }

    public static async IAsyncEnumerable<TOut> ToAsyncEnumerable<TInp, TOut>(this IAsyncStreamReader<TInp> stream,
        Func<TInp, TOut> converter,
        [EnumeratorCancellation] CancellationToken token)
    {
        while (await stream.MoveNext(token))
        {
            yield return converter(stream.Current);
        }
    }
}
