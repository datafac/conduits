using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

public interface INetChannel
{
    /// <summary>
    /// Handles a single request and returns a single result.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a single request and returns a stream of results.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a stream of requests then returns a single result.
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    ValueTask<NetResponse> ClientStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default);

    /// <summary>
    /// Simultaneously handles a stream of requests and a stream of results. These may be interleaved.
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default);
}
