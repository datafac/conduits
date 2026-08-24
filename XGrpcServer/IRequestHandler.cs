using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Testing.Calculator;

namespace XGrpcServer;

// this is suspiciously similar to IConduitBase
// todo refactor and remove this
internal interface IRequestHandler
{
    /// <summary>
    /// Handles a single request and returns a single result.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    ValueTask<ReadOnlyMemory<byte>> HandleUnaryRequest(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a single request and returns a stream of results.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    IAsyncEnumerable<ReadOnlyMemory<byte>> HandleServerStream(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, CancellationToken cancellation = default);
}
