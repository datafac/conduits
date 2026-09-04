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
    private readonly Server _server;

    private ProtobufGrpcServer(INetChannel netChannel, ServerPort serverPort)
    {
        _server = new Server() { Ports = { serverPort } };
        _server.Services.AddCodeFirst<IProtobufNetContract>(new ProtobufNetServer(netChannel));
        _server.Start();
    }

    /// <summary>
    /// Returns a new server instance bound to any unused port. Use the BoundPort property
    /// to discover the actual port assigned.
    /// </summary>
    public ProtobufGrpcServer(INetChannel netChannel) : this(netChannel, new ServerPort("localhost", ServerPort.PickUnused, ServerCredentials.Insecure)) { }

    /// <summary>
    /// Returns a new server instance bound to a specific port.
    /// </summary>
    public ProtobufGrpcServer(INetChannel netChannel, int port) : this(netChannel, new ServerPort("localhost", port, ServerCredentials.Insecure)) { }

    /// <summary>
    /// Returns the port assigned to the server.
    /// </summary>
    public int BoundPort => _server.Ports.First().BoundPort;

    public async ValueTask DisposeAsync()
    {
        await _server.ShutdownAsync().ConfigureAwait(false);
    }
}
