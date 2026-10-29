using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits;

/// <summary>
/// A router that directs messages to configured servers.
/// </summary>
public sealed class ConduitRouter : INetConduit, IAsyncDisposable
{
    private readonly struct RouteRec
    {
        public readonly ConduitServer Server;
        public readonly bool ChainDispose;
        public RouteRec(ConduitServer server, bool chainDispose)
        {
            Server = server;
            ChainDispose = chainDispose;
        }
    }

    private readonly ImmutableDictionary<int, RouteRec> _routes;

    private ConduitRouter(ImmutableDictionary<int, RouteRec> routes)
    {
        _routes = routes;
    }

    public ConduitRouter() : this(ImmutableDictionary<int, RouteRec>.Empty) { }
    public ConduitRouter(int route, ConduitServer server, bool chainDispose) : this(ImmutableDictionary<int, RouteRec>.Empty.Add(route, new RouteRec(server, chainDispose))) { }
    public ConduitRouter WithRoute(int route, ConduitServer server, bool chainDispose) => new ConduitRouter(_routes.Add(route, new RouteRec(server, chainDispose)));

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var route in _routes.Values)
        {
            if (route.ChainDispose)
            {
                await route.Server.DisposeAsync();
            }
        }
        GC.SuppressFinalize(this);
    }

    public async ValueTask<ConduitInfo> GetConduitInfoAsync()
    {
        return new ConduitInfo($"{ThisAssembly.AssemblyName}.{nameof(ConduitRouter)}", new Version(ThisAssembly.AssemblyFileVersion));
    }

    public ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var route = request.Route;
        if(_routes.TryGetValue(route, out var routeRec))
        {
            return routeRec.Server.UnaryRequest(request, deadlineUtc, cancellation);
        }
        else
        {
            return new ValueTask<NetResponse>(new NetResponse(ControlCode.InvalidOp, $"Invalid route: {route}".EncodeMsg()));
        }
    }

    private static async IAsyncEnumerable<NetResponse> SingleResponse(NetResponse response)
    {
        yield return response;
    }

    public IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var route = request.Route;
        if (_routes.TryGetValue(route, out var routeRec))
        {
            return routeRec.Server.ServerStream(request, deadlineUtc, cancellation);
        }
        else
        {
            return SingleResponse(new NetResponse(ControlCode.InvalidOp, $"Invalid route: {route}".EncodeMsg()));
        }
    }

    public ValueTask<NetResponse> ClientStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        int route = 0; // todo request.Route;
        if (_routes.TryGetValue(route, out var routeRec))
        {
            return routeRec.Server.ClientStream(requests, deadlineUtc, cancellation);
        }
        else
        {
            return new ValueTask<NetResponse>(new NetResponse(ControlCode.InvalidOp, $"Invalid route: {route}".EncodeMsg()));
        }
    }

    public IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        int route = 0; // todo request.Route;
        if (_routes.TryGetValue(route, out var routeRec))
        {
            return routeRec.Server.DuplexStream(requests, deadlineUtc, cancellation);
        }
        else
        {
            return SingleResponse(new NetResponse(ControlCode.InvalidOp, $"Invalid route: {route}".EncodeMsg()));
        }
    }
}
