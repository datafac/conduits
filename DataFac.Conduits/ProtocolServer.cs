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
public sealed class ProtocolServer : INetChannel, IAsyncDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserChannel _userChannel;

    public ProtocolServer(TimeProvider? timeProvider, IUserChannel userChannel)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        _userChannel = userChannel;
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_userChannel is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }

    public TimeProvider TimeProvider => _timeProvider;

    public string ServerName => ThisAssembly.AssemblyName;

    public string ServerVersion => ThisAssembly.AssemblyFileVersion;

    private static ReadOnlyMemory<byte> EncodeErrorMessage(string message)
    {
        if (message.Length == 0) return ReadOnlyMemory<byte>.Empty;
        return Encoding.UTF8.GetBytes(message);
    }

    public async ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var response = await _userChannel.UnaryRequest(new UserRequest(request.Payload), cancellation);
        return new NetResponse(response.Payload);
    }

    public async IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (var response in _userChannel.ServerStream(new UserRequest(request.Payload), cancellation))
        {
            if (cancellation.IsCancellationRequested)
            {
                // cancelled
                yield return new NetResponse(ControlCode.Cancelled, EncodeErrorMessage("Operation cancelled"));
                yield break;
            }
            else if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
            {
                // deadline exceeded
                yield return new NetResponse(ControlCode.Timeout, EncodeErrorMessage("Deadline exceeded"));
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

        async IAsyncEnumerable<UserRequest> ToUserRequests()
        {
            await foreach (var request in requests)
            {
                if (cancellation.IsCancellationRequested)
                {
                    // cancelled
                    altResponse = new NetResponse(ControlCode.Cancelled, EncodeErrorMessage("Operation cancelled"));
                    yield break;
                }
                else if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
                {
                    // deadline exceeded
                    altResponse = new NetResponse(ControlCode.Timeout, EncodeErrorMessage("Deadline exceeded"));
                    yield break;
                }
                else
                {
                    yield return new UserRequest(request.Payload);
                }
            }
        }

        var response = await _userChannel.ClientStream(ToUserRequests(), cancellation);

        return altResponse.HasValue
            ? altResponse.Value
            : new NetResponse(response.Payload);
    }

    public async IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        NetResponse? altResponse = null;

        async IAsyncEnumerable<UserRequest> ToUserRequests()
        {
            await foreach (var request in requests)
            {
                if (cancellation.IsCancellationRequested)
                {
                    // cancelled
                    altResponse = new NetResponse(ControlCode.Cancelled, EncodeErrorMessage("Operation cancelled"));
                    yield break;
                }
                else if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
                {
                    // deadline exceeded
                    altResponse = new NetResponse(ControlCode.Timeout, EncodeErrorMessage("Deadline exceeded"));
                    yield break;
                }
                else
                {
                    yield return new UserRequest(request.Payload);
                }
            }
        }

        await foreach (var response in _userChannel.DuplexStream(ToUserRequests(), cancellation))
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
                yield return new NetResponse(ControlCode.Cancelled, EncodeErrorMessage("Operation cancelled"));
                yield break;
            }
            else if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
            {
                // deadline exceeded
                yield return new NetResponse(ControlCode.Timeout, EncodeErrorMessage("Deadline exceeded"));
                yield break;
            }
            else
            {
                yield return new NetResponse(response.Payload);
            }
        }
    }
}
