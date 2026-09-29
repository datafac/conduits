using Grpc.Core;
using ProtoBuf.Grpc;
using System;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Server;
using DataFac.Conduits.ProtobufNetCommon;
using System.Linq;

namespace DataFac.Conduits.ProtobufNetServer;


/// <summary>
/// Implements a ProtobufNet Grpc server.
/// </summary>
public sealed class ProtobufGrpcServer : IAsyncDisposable
{
    private readonly INetConduit _netConduit;
    private readonly bool _chainDispose;
    private readonly Server _server;

    private ProtobufGrpcServer(INetConduit netConduit, bool chainDispose, ServerPort serverPort)
    {
        _netConduit = netConduit;
        _chainDispose = chainDispose;
        _server = new Server() { Ports = { serverPort } };
        _server.Services.AddCodeFirst<IProtobufNetContract>(new ProtobufNetServer(netConduit));
        _server.Start();
    }

    /// <summary>
    /// Returns a new server instance bound to any unused port. Use the BoundPort property
    /// to discover the actual port assigned.
    /// </summary>
    public ProtobufGrpcServer(INetConduit netChannel, bool chainDispose = false) : this(netChannel, chainDispose, new ServerPort("localhost", ServerPort.PickUnused, ServerCredentials.Insecure)) { }

    /// <summary>
    /// Returns a new server instance bound to a specific port.
    /// </summary>
    public ProtobufGrpcServer(INetConduit netChannel, bool chainDispose = false, int port = 0) : this(netChannel, chainDispose, new ServerPort("localhost", port, ServerCredentials.Insecure)) { }

    /// <summary>
    /// Returns the port assigned to the server.
    /// </summary>
    public int BoundPort => _server.Ports.First().BoundPort;

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await _server.ShutdownAsync().ConfigureAwait(false);
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
}
