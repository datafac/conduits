using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

public interface IAppConduit
{
    /// <summary>
    /// Returns information about the server application, such as its name, version, and other metadata.
    /// </summary>
    /// <returns></returns>
    ValueTask<string> GetAppInfo();

    /// <summary>
    /// Handles a single request and returns a single result.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    ValueTask<AppResponse> UnaryRequest(AppRequest request, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a single request and returns a stream of results.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    IAsyncEnumerable<AppResponse> ServerStream(AppRequest request, CancellationToken cancellation = default);

    /// <summary>
    /// Handles a stream of requests then returns a single result.
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    ValueTask<AppResponse> ClientStream(IAsyncEnumerable<AppRequest> requests, CancellationToken cancellation = default);

    /// <summary>
    /// Simultaneously handles a stream of requests and a stream of results. These may be interleaved.
    /// </summary>
    /// <param name="requests"></param>
    /// <param name="deadlineUtc"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    IAsyncEnumerable<AppResponse> DuplexStream(IAsyncEnumerable<AppRequest> requests, CancellationToken cancellation = default);
}
