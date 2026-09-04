using DataFac.Conduits.GrpcCommon;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataFac.Conduits.GrpcServer;

public class GrpcService : DataFac.Conduits.GrpcCommon.GrpcService.GrpcServiceBase
{
    private readonly INetChannel _server;
    public GrpcService(INetChannel server)
    {
        _server = server;
    }

    public override async Task<GrpcPayload> NoStream(GrpcPayload request, ServerCallContext context)
    {
        var result = await _server.UnaryRequest(request.ToConduitRequest(), context.Deadline, context.CancellationToken);
        return result.ToGrpcPayload();
    }

    public override async Task StreamDn(GrpcPayload request, IServerStreamWriter<GrpcPayload> responseStream, ServerCallContext context)
    {
        await foreach (var response in _server.ServerStream(request.ToConduitRequest(), context.Deadline, context.CancellationToken))
        {
            await responseStream.WriteAsync(response.ToGrpcPayload());
        }
    }

    public override async Task<GrpcPayload> StreamUp(IAsyncStreamReader<GrpcPayload> requestStream, ServerCallContext context)
    {
        IAsyncEnumerable<NetRequest> requests = requestStream.ToAsyncEnumerable((i) => i.ToConduitRequest(), context.CancellationToken);
        var response = await _server.ClientStream(requests, context.Deadline, context.CancellationToken);
        return response.ToGrpcPayload();
    }

    public override async Task BiStream(IAsyncStreamReader<GrpcPayload> requestStream, IServerStreamWriter<GrpcPayload> responseStream, ServerCallContext context)
    {
        var requests = requestStream.ToAsyncEnumerable((i) => i.ToConduitRequest(), context.CancellationToken);
        await foreach (var response in _server.DuplexStream(requests, context.Deadline, context.CancellationToken))
        {
            await responseStream.WriteAsync(response.ToGrpcPayload());
        }
    }
}
