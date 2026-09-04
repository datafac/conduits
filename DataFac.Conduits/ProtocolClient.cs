using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

/// <summary>
/// Implements client-side conduit protocol.
/// </summary>
public sealed class ProtocolClient : IUserChannel, IAsyncDisposable
{
    private readonly INetChannel _netChannel;

    private readonly TimeProvider _timeProvider;
    public TimeProvider TimeProvider => _timeProvider;

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

    public ProtocolClient(INetChannel netChannel, TimeSpan? maxCallDuration = null, TimeProvider? timeProvider = null)
    {
        _netChannel = netChannel;
        _maxCallDuration = SanitiseMaxCallDuration(maxCallDuration);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async ValueTask DisposeAsync()
    {
        if (_netChannel is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

    private DateTime? calculateDeadline()
    {
        return _maxCallDuration.HasValue
            ? _timeProvider.GetUtcNow().UtcDateTime + _maxCallDuration.Value
            : null;
    }

    private static ReadOnlyMemory<byte> EncodeErrorMessage(string message)
    {
        if (message.Length == 0) return ReadOnlyMemory<byte>.Empty;
        return Encoding.UTF8.GetBytes(message);
    }

    private static string DecodeErrorMessage(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 0) return string.Empty;
        return Encoding.UTF8.GetString(payload.ToArray());
    }

    private static NetResponse HandleResponse(NetResponse response)
    {
        return response.Control switch
        {
            ControlCode.None => response,
            ControlCode.Timeout => throw new TimeoutException(DecodeErrorMessage(response.Payload.Span)),
            ControlCode.Cancelled => throw new OperationCanceledException(DecodeErrorMessage(response.Payload.Span)),
            _ => throw new Exception($"Unknown control code: {response.Control}")
        };
    }

    public async ValueTask<UserResponse> UnaryRequest(UserRequest request, CancellationToken cancellation = default)
    {
        DateTime? deadline = calculateDeadline();
        var response = await _netChannel.UnaryRequest(new NetRequest(request.Payload), deadline).ConfigureAwait(false);
        var result = HandleResponse(response);
        return new UserResponse(result.Payload);
    }

    public async IAsyncEnumerable<UserResponse> ServerStream(UserRequest request, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        DateTime? deadline = calculateDeadline();
        await foreach (var response in _netChannel.ServerStream(new NetRequest(request.Payload), deadline, cancellation).ConfigureAwait(false))
        {
            var result = HandleResponse(response);
            yield return new UserResponse(result.Payload);
        }
    }

    public async ValueTask<UserResponse> ClientStream(IAsyncEnumerable<UserRequest> requests, CancellationToken cancellation = default)
    {
        DateTime? deadline = calculateDeadline();

        async IAsyncEnumerable<NetRequest> ToNetRequests()
        {
            await foreach (var request in requests)
            {
                yield return new NetRequest(request.Payload);
            }
        }

        var response = await _netChannel.ClientStream(ToNetRequests(), deadline).ConfigureAwait(false);
        var result = HandleResponse(response);
        return new UserResponse(result.Payload);
    }

    public async IAsyncEnumerable<UserResponse> DuplexStream(IAsyncEnumerable<UserRequest> requests, [EnumeratorCancellation] CancellationToken cancellation = default)
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

        await foreach (var response in _netChannel.DuplexStream(ToNetRequests(), deadline, cancellation).ConfigureAwait(false))
        {
            var result = HandleResponse(response);
            yield return new UserResponse(result.Payload);
        }
    }

}
