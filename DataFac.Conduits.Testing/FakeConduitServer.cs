using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.Testing;

public sealed class FakeConduitServer
{
    private readonly INetConduit _netConduit;
    private readonly bool _chainDispose;

    /// <summary>
    /// Creates a test/mock server wrapping another conduit server.
    /// </summary>
    /// <param name="server">The wrapped server.</param>
    public FakeConduitServer(INetConduit netConduit, bool chainDispose = false)
    {
        _netConduit = netConduit ?? throw new ArgumentNullException(nameof(netConduit));
        _chainDispose = chainDispose;
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

    public ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _netConduit.UnaryRequest(request, deadlineUtc, cancellation);
    }

    public IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _netConduit.ServerStream(request, deadlineUtc, cancellation);
    }

    public ValueTask<NetResponse> ClientStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _netConduit.ClientStream(requests, deadlineUtc, cancellation);
    }

    public IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _netConduit.DuplexStream(requests, deadlineUtc, cancellation);
    }
}
