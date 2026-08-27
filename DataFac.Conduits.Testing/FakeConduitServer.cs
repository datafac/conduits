using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.Testing;

public sealed class FakeConduitServer : IConduitServer
{
    private readonly TimeProvider _timeProvider;
    public TimeProvider TimeProvider => _timeProvider;
    private readonly IConduitServer _server;

    public string ServerName => ThisAssembly.AssemblyName;
    public string ServerVersion => ThisAssembly.AssemblyVersion;

    /// <summary>
    /// Creates a test/mock server wrapping another conduit server.
    /// </summary>
    /// <param name="server">The wrapped server.</param>
    public FakeConduitServer(IConduitServer server, TimeProvider? timeProvider)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        // nothing to dispose yet
    }

    public IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.ServerStream(request, deadlineUtc, cancellation);
    }

    public ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.SimpleUnaryCall(request, deadlineUtc, cancellation);
    }

    public ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.ClientStream(requests, deadlineUtc, cancellation);
    }

    public IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FakeConduitServer));
        return _server.DuplexStream(requests, deadlineUtc, cancellation);
    }
}
