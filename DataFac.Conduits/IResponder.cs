using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

public interface IResponder
{
    /// <summary>
    /// Handles a single request and returns a single result.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    ValueTask<UserResponse> UnaryRequest(UserRequest request, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a single request and returns a stream of results.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    IAsyncEnumerable<UserResponse> ServerStream(UserRequest request, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a stream of requests then returns a single result.
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    ValueTask<UserResponse> ClientStream(IAsyncEnumerable<UserRequest> requests, CancellationToken cancellation = default);

    /// <summary>
    /// Simultaneously handles a stream of requests and a stream of results. These may be interleaved.
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    IAsyncEnumerable<UserResponse> DuplexStream(IAsyncEnumerable<UserRequest> requests, CancellationToken cancellation = default);
}
