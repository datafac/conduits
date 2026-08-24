using Grpc.Core;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Server;
using DataFac.Conduits.ProtobufNetCommon;
using System.Linq;

namespace DataFac.Conduits.ProtobufNetServer
{
    internal class ProtobufNetServer : IProtobufNetContract
    {
        private readonly IConduitServer _requestHandler;

        public ProtobufNetServer(IConduitServer requestHandler)
        {
            _requestHandler = requestHandler;
        }

        private static TimeSpan GetMaxCallDuration(CallContext context)
        {
            DateTime deadline = context.Deadline ?? DateTime.UtcNow.AddHours(1);
            TimeSpan duration = deadline - DateTime.UtcNow;
            if (duration > TimeSpan.FromHours(1))
            {
                duration = TimeSpan.FromHours(1);
            }
            return duration;
        }

        public async ValueTask<ResultBlob> UnaryRequest(RequestBlob requestBlob, CallContext context)
        {
            TimeSpan timeout = GetMaxCallDuration(context);
            using var deadlineCts = new CancellationTokenSource(timeout);
            using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, deadlineCts.Token);
            var result = await _requestHandler.SimpleUnaryCall(requestBlob.Blob, context.Deadline, requestCts.Token);
            return new ResultBlob() { Blob = result.ToArray() }; // todo remove ToArray()
        }

        public async IAsyncEnumerable<ResultBlob> ServerStream(RequestBlob requestBlob, CallContext context)
        {
            TimeSpan timeout = GetMaxCallDuration(context);
            using var deadlineCts = new CancellationTokenSource(timeout);
            using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, deadlineCts.Token);
            await foreach (var result in _requestHandler.ServerStream(requestBlob.Blob, context.Deadline, requestCts.Token))
            {
                yield return new ResultBlob() { Blob = result.ToArray() }; // todo remove ToArray()
            }
        }

        public ValueTask<ResultBlob> ClientStream(IAsyncEnumerable<RequestBlob> requests, CallContext context = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<ResultBlob> DuplexStream(IAsyncEnumerable<RequestBlob> requests, CallContext context = default)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Implements a ProtobufNet Grpc server.
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
}
