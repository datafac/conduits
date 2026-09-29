using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

/// <summary>
/// Implements server-side conduit protocol.
/// </summary>
public sealed class ConduitServer : INetConduit, IAsyncDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly IAppConduit _appConduit;
    private readonly bool _chainDispose;

    public ConduitServer(TimeProvider? timeProvider, IAppConduit appConduit, bool chainDispose = false)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        _appConduit = appConduit;
        _chainDispose = chainDispose;
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_chainDispose)
        {
            if (_appConduit is IAsyncDisposable disposable1)
            {
                await disposable1.DisposeAsync();
            }
            else if (_appConduit is IDisposable disposable2)
            {
                disposable2.Dispose();
            }
        }
        GC.SuppressFinalize(this);
    }
    public TimeProvider TimeProvider => _timeProvider;

    public async ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (request.Control == ControlCode.GetAppInfo)
        {
            var appInfo = await _appConduit.GetAppInfo();
            return new NetResponse(ControlCode.GetAppInfo, appInfo.EncodeMsg());
        }
        else
        {
            var response = await _appConduit.UnaryRequest(new AppRequest(request.Payload), cancellation);
            return new NetResponse(response.Payload);
        }
    }

    public async IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (var response in _appConduit.ServerStream(new AppRequest(request.Payload), cancellation))
        {
            if (cancellation.IsCancellationRequested)
            {
                // cancelled
                yield return new NetResponse(ControlCode.Cancelled, "Operation cancelled".EncodeMsg());
                yield break;
            }
            else if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
            {
                // deadline exceeded
                yield return new NetResponse(ControlCode.Timeout, "Deadline exceeded".EncodeMsg());
                yield break;
            }
            else
            {
                yield return new NetResponse(response.Payload);
            }
        }
    }

    public async ValueTask<NetResponse> ClientStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        NetResponse? altResponse = null;

        async IAsyncEnumerable<AppRequest> ToAppRequests()
        {
            await foreach (var request in requests)
            {
                if (cancellation.IsCancellationRequested)
                {
                    // cancelled
                    altResponse = new NetResponse(ControlCode.Cancelled, "Operation cancelled".EncodeMsg());
                    yield break;
                }
                else if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
                {
                    // deadline exceeded
                    altResponse = new NetResponse(ControlCode.Timeout, "Deadline exceeded".EncodeMsg());
                    yield break;
                }
                else
                {
                    yield return new AppRequest(request.Payload);
                }
            }
        }

        var response = await _appConduit.ClientStream(ToAppRequests(), cancellation);

        return altResponse.HasValue
            ? altResponse.Value
            : new NetResponse(response.Payload);
    }

    public async IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        NetResponse? altResponse = null;

        async IAsyncEnumerable<AppRequest> ToAppRequests()
        {
            await foreach (var request in requests)
            {
                if (cancellation.IsCancellationRequested)
                {
                    // cancelled
                    altResponse = new NetResponse(ControlCode.Cancelled, "Operation cancelled".EncodeMsg());
                    yield break;
                }
                else if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
                {
                    // deadline exceeded
                    altResponse = new NetResponse(ControlCode.Timeout, "Deadline exceeded".EncodeMsg());
                    yield break;
                }
                else
                {
                    yield return new AppRequest(request.Payload);
                }
            }
        }

        await foreach (var response in _appConduit.DuplexStream(ToAppRequests(), cancellation))
        {
            if (altResponse.HasValue)
            {
                // client stream did not complete
                yield return altResponse.Value;
                yield break;
            }
            else if (cancellation.IsCancellationRequested)
            {
                // cancelled
                yield return new NetResponse(ControlCode.Cancelled, "Operation cancelled".EncodeMsg());
                yield break;
            }
            else if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
            {
                // deadline exceeded
                yield return new NetResponse(ControlCode.Timeout, "Deadline exceeded".EncodeMsg());
                yield break;
            }
            else
            {
                yield return new NetResponse(response.Payload);
            }
        }
    }
}
