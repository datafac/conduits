using DataFac.Conduits.GrpcCommon;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DataFac.Conduits.GrpcServer
{
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
            ReadOnlyMemory<byte> result = await _server.SimpleUnaryCall(request.ToPayload(), new CallContext(context.CancellationToken, context.Deadline));
            return result.ToGrpcPayload();
        }

        public override async Task StreamDn(GrpcPayload request, IServerStreamWriter<GrpcPayload> responseStream, ServerCallContext context)
        {
            await foreach (var response in _server.ServerStream(request.ToPayload(), new CallContext(context.CancellationToken, context.Deadline)))
            {
                await responseStream.WriteAsync(response.ToGrpcPayload());
            }
        }

        public override async Task<GrpcPayload> StreamUp(IAsyncStreamReader<GrpcPayload> requestStream, ServerCallContext context)
        {
            var requests = requestStream.ToAsyncEnumerable((i) => i.ToPayload(), context.CancellationToken);
            var incoming = await _server.ClientStream(requests, new CallContext(context.CancellationToken, context.Deadline));
            return incoming.ToGrpcPayload();
        }

        public override async Task BiStream(IAsyncStreamReader<GrpcPayload> requestStream, IServerStreamWriter<GrpcPayload> responseStream, ServerCallContext context)
        {
            var requests = requestStream.ToAsyncEnumerable((i) => i.ToPayload(), context.CancellationToken);
            await foreach (var response in _server.DuplexStream(requests, new CallContext(context.CancellationToken, context.Deadline)))
            {
                await responseStream.WriteAsync(response.ToGrpcPayload());
            }
        }
    }
}