using System;
using System.Linq;
using System.Threading.Tasks;
using DataFac.Conduits;
using DataFac.Conduits.ProtobufNet.Common;
using Grpc.Core;
using ProtoBuf.Grpc.Server;

namespace XGrpcServer;

/// <summary>
/// Implements a simple calculator server.
/// </summary>
public sealed class ProtobufGrpcServer : IAsyncDisposable
{
    private readonly Server _server;

    private ProtobufGrpcServer(IConduitServer conduitServer, ServerPort serverPort)
    {
        _server = new Server() { Ports = { serverPort } };
        _server.Services.AddCodeFirst<IProtobufNetContract>(new ProtobufNetServer(conduitServer));
        _server.Start();
    }

    /// <summary>
    /// Returns a new server instance bound to any unused port. Use the BoundPort property
    /// to discover the actual port assigned.
    /// </summary>
    public ProtobufGrpcServer(IConduitServer conduitServer) : this(conduitServer, new ServerPort("localhost", ServerPort.PickUnused, ServerCredentials.Insecure)) { }

    /// <summary>
    /// Returns a new server instance bound to a specific port.
    /// </summary>
    public ProtobufGrpcServer(IConduitServer conduitServer, int port) : this(conduitServer, new ServerPort("localhost", port, ServerCredentials.Insecure)) { }

    /// <summary>
    /// Returns the port assigned to the server.
    /// </summary>
    public int BoundPort => _server.Ports.First().BoundPort;

    public async ValueTask DisposeAsync()
    {
        await _server.ShutdownAsync().ConfigureAwait(false);
    }
}
