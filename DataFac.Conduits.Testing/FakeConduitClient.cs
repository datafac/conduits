using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.Testing;

public class FakeConduitClient : IConduitClient, IAsyncDisposable
{
    private readonly FakeConduitServer _server;
    private readonly TimeProvider _timeProvider;
    public TimeProvider TimeProvider => _timeProvider;

    public FakeConduitClient(FakeConduitServer server, TimeProvider? timeProvider)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
    }

    public IAsyncEnumerable<ConduitResponse> ServerStream(ConduitRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.ServerStream(request, deadlineUtc, cancellation);
    }

    public ValueTask<ConduitResponse> UnaryRequest(ConduitRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.UnaryRequest(request, deadlineUtc, cancellation);
    }

    public ValueTask<ConduitResponse> ClientStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.ClientStream(requests, deadlineUtc, cancellation);
    }

    public IAsyncEnumerable<ConduitResponse> DuplexStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.DuplexStream(requests, deadlineUtc, cancellation);
    }
}
