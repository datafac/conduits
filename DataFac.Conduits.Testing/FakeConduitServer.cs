using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.Testing;

public sealed class FakeConduitServer
{
    private readonly INetChannel _server;

    public string ServerName => ThisAssembly.AssemblyName;
    public string ServerVersion => ThisAssembly.AssemblyVersion;

    /// <summary>
    /// Creates a test/mock server wrapping another conduit server.
    /// </summary>
    /// <param name="server">The wrapped server.</param>
    public FakeConduitServer(INetChannel server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if(_server is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }

    public ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.UnaryRequest(request, deadlineUtc, cancellation);
    }

    public IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.ServerStream(request, deadlineUtc, cancellation);
    }

    public ValueTask<NetResponse> ClientStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.ClientStream(requests, deadlineUtc, cancellation);
    }

    public IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.DuplexStream(requests, deadlineUtc, cancellation);
    }
}
