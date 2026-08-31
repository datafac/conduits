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
public sealed class ConduitServer : IConduitServer
{
    private readonly TimeProvider _timeProvider;
    private readonly IResponder _responder;

    public ConduitServer(TimeProvider? timeProvider, IResponder responder)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        _responder = responder;
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_responder is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }

    public TimeProvider TimeProvider => _timeProvider;

    public string ServerName => ThisAssembly.AssemblyName;

    public string ServerVersion => ThisAssembly.AssemblyFileVersion;

    private static async IAsyncEnumerable<UserRequest> ToUserRequests(IAsyncEnumerable<ConduitRequest> requests)
    {
        await foreach (var request in requests)
        {
            yield return new UserRequest(request.Payload);
        }
    }

    private static ReadOnlyMemory<byte> EncodeErrorMessage(string message)
    {
        if (message.Length == 0) return ReadOnlyMemory<byte>.Empty;
        return Encoding.UTF8.GetBytes(message);
    }

    // todo implement protcol, deadlines and cancellations
    public async ValueTask<ConduitResponse> UnaryRequest(ConduitRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var response = await _responder.UnaryRequest(new UserRequest(request.Payload), cancellation);
        return new ConduitResponse(response.Payload);
    }

    public async IAsyncEnumerable<ConduitResponse> ServerStream(ConduitRequest request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (var response in _responder.ServerStream(new UserRequest(request.Payload), cancellation))
        {
            yield return new ConduitResponse(response.Payload);

            // check if cancelled
            if (cancellation.IsCancellationRequested)
            {
                yield return new ConduitResponse(ControlCode.Cancelled, EncodeErrorMessage("Operation cancelled")); // todo static
                yield break;
            }

            // check if deadline exceeded
            if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
            {
                yield return new ConduitResponse(ControlCode.Timeout, EncodeErrorMessage("Deadline exceeded"));
                yield break;
            }
        }
    }

    public async ValueTask<ConduitResponse> ClientStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var response = await _responder.ClientStream(ToUserRequests(requests), cancellation);
        return new ConduitResponse(response.Payload);
    }

    public async IAsyncEnumerable<ConduitResponse> DuplexStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (var response in _responder.DuplexStream(ToUserRequests(requests), cancellation))
        {
            yield return new ConduitResponse(response.Payload);

            // check if cancelled
            if (cancellation.IsCancellationRequested)
            {
                yield return new ConduitResponse(ControlCode.Cancelled, EncodeErrorMessage("Operation cancelled"));
                yield break;
            }

            // check if deadline exceeded
            if (deadlineUtc.HasValue && _timeProvider.GetUtcNow().UtcDateTime > deadlineUtc.Value)
            {
                yield return new ConduitResponse(ControlCode.Timeout, EncodeErrorMessage("Deadline exceeded"));
                yield break;
            }
        }
    }
}
