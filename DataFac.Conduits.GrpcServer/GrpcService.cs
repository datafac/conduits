using DataFac.Conduits.GrpcCommon;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataFac.Conduits.GrpcServer;

public class GrpcService : DataFac.Conduits.GrpcCommon.GrpcService.GrpcServiceBase
{
    private readonly ILogger<GrpcService> _logger;
    private readonly IConduitServer _server;
    public GrpcService(ILogger<GrpcService> logger, IConduitServer server)
    {
        _logger = logger;
        _server = server;
    }

    public override async Task<GrpcPayload> NoStream(GrpcPayload request, ServerCallContext context)
    {
        var result = await _server.SimpleUnaryCall(new ConduitRequest(request.Data.Memory), context.Deadline, context.CancellationToken);
        return result.Payload.ToGrpcPayload();
    }

    public override async Task StreamDn(GrpcPayload request, IServerStreamWriter<GrpcPayload> responseStream, ServerCallContext context)
    {
        await foreach (var response in _server.ServerStream(new ConduitRequest(request.Data.Memory), context.Deadline, context.CancellationToken))
        {
            await responseStream.WriteAsync(response.Payload.ToGrpcPayload());
        }
    }

    public override async Task<GrpcPayload> StreamUp(IAsyncStreamReader<GrpcPayload> requestStream, ServerCallContext context)
    {
        IAsyncEnumerable<ConduitRequest> requests = requestStream.ToAsyncEnumerable((i) => new ConduitRequest(i.Data.Memory), context.CancellationToken);
        var incoming = await _server.ClientStream(requests, context.Deadline, context.CancellationToken);
        return incoming.ToGrpcPayload();
    }

    public override async Task BiStream(IAsyncStreamReader<GrpcPayload> requestStream, IServerStreamWriter<GrpcPayload> responseStream, ServerCallContext context)
    {
        var requests = requestStream.ToAsyncEnumerable((i) => new ConduitRequest(i.Data.Memory), context.CancellationToken);
        await foreach (var response in _server.DuplexStream(requests, context.Deadline, context.CancellationToken))
        {
            await responseStream.WriteAsync(response.ToGrpcPayload());
        }
    }
}
