using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

/// <summary>
/// Implements client-side conduit protocol.
/// </summary>
public sealed class ConduitClient : IAppConduit, IAsyncDisposable
{
    private readonly INetConduit _netConduit;
    private readonly bool _chainDispose;

    private readonly TimeProvider _timeProvider;
    public TimeProvider TimeProvider => _timeProvider;

    public ConduitClient(INetConduit netConduit, bool chainDispose = false, TimeSpan? maxCallDuration = null, TimeProvider? timeProvider = null) // todo default false
    {
        _netConduit = netConduit;
        _chainDispose = chainDispose;
        _maxCallDuration = SanitiseMaxCallDuration(maxCallDuration);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    private static TimeSpan? SanitiseMaxCallDuration(TimeSpan? value)
    {
        if (value is null) return null;
        else if (value < TimeSpan.Zero) return TimeSpan.Zero;
        else if (value > TimeSpan.FromSeconds(300)) return TimeSpan.FromSeconds(300);
        else return value;
    }

    private TimeSpan? _maxCallDuration;

    /// <summary>
    /// A duration between 0 and 5 minutes, or null. If not null, this is used to calculate 
    /// a deadline for each call.
    /// </summary>
    public TimeSpan? MaxCallDuration
    {
        get => _maxCallDuration;
        set => _maxCallDuration = SanitiseMaxCallDuration(value);
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_chainDispose)
        {
            if (_netConduit is IAsyncDisposable disposable1)
            {
                await disposable1.DisposeAsync();
            }
            else if (_netConduit is IDisposable disposable2)
            {
                disposable2.Dispose();
            }
        }
        GC.SuppressFinalize(this);
    }

    private DateTime? calculateDeadline()
    {
        return _maxCallDuration.HasValue
            ? _timeProvider.GetUtcNow().UtcDateTime + _maxCallDuration.Value
            : null;
    }

    private static NetResponse HandleResponse(NetResponse response)
    {
        return response.Control switch
        {
            ControlCode.None => response,
            ControlCode.GetAppInfo => response,
            ControlCode.Timeout => throw new TimeoutException(response.Payload.DecodeMsg()),
            ControlCode.Cancelled => throw new OperationCanceledException(response.Payload.DecodeMsg()),
            ControlCode.InvalidOp => throw new InvalidOperationException(response.Payload.DecodeMsg()),
            ControlCode.InvalidData => throw new InvalidDataException(response.Payload.DecodeMsg()),
            _ => throw new Exception($"Unknown control code: {response.Control}")
        };
    }


    public async ValueTask<string> GetAppInfo()
    {
        var response = await _netConduit.UnaryRequest(new NetRequest(ControlCode.GetAppInfo), null).ConfigureAwait(false);
        var result = HandleResponse(response);
        return result.Payload.DecodeMsg();
    }

    public async ValueTask<AppResponse> UnaryRequest(AppRequest request, CancellationToken cancellation = default)
    {
        DateTime? deadline = calculateDeadline();
        var response = await _netConduit.UnaryRequest(new NetRequest(request.Payload), deadline).ConfigureAwait(false);
        var result = HandleResponse(response);
        return new AppResponse(result.Payload);
    }

    public async IAsyncEnumerable<AppResponse> ServerStream(AppRequest request, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        DateTime? deadline = calculateDeadline();
        await foreach (var response in _netConduit.ServerStream(new NetRequest(request.Payload), deadline, cancellation).ConfigureAwait(false))
        {
            var result = HandleResponse(response);
            yield return new AppResponse(result.Payload);
        }
    }

    public async ValueTask<AppResponse> ClientStream(IAsyncEnumerable<AppRequest> requests, CancellationToken cancellation = default)
    {
        DateTime? deadline = calculateDeadline();

        async IAsyncEnumerable<NetRequest> ToNetRequests()
        {
            await foreach (var request in requests)
            {
                yield return new NetRequest(request.Payload);
            }
        }

        var response = await _netConduit.ClientStream(ToNetRequests(), deadline).ConfigureAwait(false);
        var result = HandleResponse(response);
        return new AppResponse(result.Payload);
    }

    public async IAsyncEnumerable<AppResponse> DuplexStream(IAsyncEnumerable<AppRequest> requests, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        DateTime? deadline = calculateDeadline();

        // todo? does this support interleaving of requests and responses? 
        async IAsyncEnumerable<NetRequest> ToNetRequests()
        {
            await foreach (var request in requests)
            {
                yield return new NetRequest(request.Payload);
            }
        }

        await foreach (var response in _netConduit.DuplexStream(ToNetRequests(), deadline, cancellation).ConfigureAwait(false))
        {
            var result = HandleResponse(response);
            yield return new AppResponse(result.Payload);
        }
    }
}
