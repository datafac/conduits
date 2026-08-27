using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

public readonly struct ConduitRequest
{
    public readonly ReadOnlyMemory<byte> Payload;

    public ConduitRequest(ReadOnlyMemory<byte> payload)
    {
        Payload = payload;
    }
}

public interface IConduitBase
{
    /// <summary>
    /// Handles a single request and returns a single result.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ConduitRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a single request and returns a stream of results.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a stream of requests then returns a single result.
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default);

    /// <summary>
    /// Simultaneously handles a stream of requests and a stream of results. These may be interleaved.
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default);
}
