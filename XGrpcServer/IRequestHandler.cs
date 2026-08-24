using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Testing.Calculator;
using XGrpcShared;

namespace XGrpcServer;

internal interface IRequestHandler
{
    /// <summary>
    /// Handles a single request and returns a single result.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    ValueTask<ReadOnlyMemory<byte>> HandleUnaryRequest(ReadOnlyMemory<byte> request, CancellationToken cancellation);

    /// <summary>
    /// Handles a single request and returns a stream of results.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    IAsyncEnumerable<ResultBase> HandleServerStream(RequestBase request, CancellationToken cancellation);
}
