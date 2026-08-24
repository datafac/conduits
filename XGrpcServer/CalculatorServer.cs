using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using ProtoBuf.Grpc.Server;
using XGrpcShared;

namespace XGrpcServer;

/// <summary>
/// Implements a simple calculator server.
/// </summary>
public sealed class CalculatorServer : IAsyncDisposable
{
    private readonly Server _server;

    private CalculatorServer(ServerPort serverPort)
    {
        _server = new Server() { Ports = { serverPort } };
        _server.Services.AddCodeFirst<IConduit>(new ConduitServer(new RequestHandler(new Calculator())));
        _server.Start();
    }

    /// <summary>
    /// Returns a new server instance bound to any unused port. Use the BoundPort property
    /// to discover the actual port assigned.
    /// </summary>
    public CalculatorServer() : this(new ServerPort("localhost", ServerPort.PickUnused, ServerCredentials.Insecure)) { }

    /// <summary>
    /// Returns a new server instance bound to a specific port.
    /// </summary>
    public CalculatorServer(int port) : this(new ServerPort("localhost", port, ServerCredentials.Insecure)) { }

    /// <summary>
    /// Returns the port assigned to the server.
    /// </summary>
    public int BoundPort => _server.Ports.First().BoundPort;

    public async ValueTask DisposeAsync()
    {
        await _server.ShutdownAsync().ConfigureAwait(false);
    }
}
