using DataFac.Conduits.GrpcCommon;
using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.GrpcClient
{
    public class GrpcConduitClient : IDisposable, IConduitClient
    {
        private readonly GrpcChannel _channel;

        public GrpcConduitClient(Uri address)
        {
            _channel = GrpcChannel.ForAddress(address);
        }

        private volatile bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _channel?.Dispose();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowDisposed()
        {
            throw new ObjectDisposedException(nameof(GrpcConduitClient));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckNotDisposed()
        {
            if (_disposed) ThrowDisposed();
        }

        public async ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ReadOnlyMemory<byte> request, CallContext context)
        {
            CheckNotDisposed();
            var client = new GrpcService.GrpcServiceClient(_channel);
            var incoming = await client.NoStreamAsync(new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(request) }, cancellationToken: context.Token, deadline: context.DeadlineUtc);
            return incoming.Data.Memory;
        }

        public async IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> request, CallContext context)
        {
            CheckNotDisposed();
            var client = new GrpcService.GrpcServiceClient(_channel);
            var call = client.StreamDn(new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(request) }, cancellationToken: context.Token, deadline: context.DeadlineUtc);

            var responseStream = call.ResponseStream;
            while (await responseStream.MoveNext(context.Token) && !context.Token.IsCancellationRequested)
            {
                yield return responseStream.Current.Data.Memory;
            }
        }

        public async ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
        {
            CheckNotDisposed();
            var client = new GrpcService.GrpcServiceClient(_channel);
            using var call = client.StreamUp(deadline: deadlineUtc, cancellationToken: cancellation);
            var pushTask = Task.Run(async () =>
            {
                var requestStream = call.RequestStream;
                await foreach (var request in requests)
                {
                    await requestStream.WriteAsync(new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(request) });
                }

                await requestStream.CompleteAsync();
            });
            await Task.WhenAll(pushTask);
            var incoming = await call.ResponseAsync;
            return incoming.Data.Memory;
        }

        public async IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
        {
            CheckNotDisposed();
            var client = new GrpcService.GrpcServiceClient(_channel);
            using var call = client.BiStream(cancellationToken: cancellation, deadline: deadlineUtc);

            var pushTask = Task.Run(async () =>
            {
                var requestStream = call.RequestStream;
                await foreach (var request in requests)
                {
                    await requestStream.WriteAsync(new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(request) });
                }

                await requestStream.CompleteAsync();
            });

            var responseStream = call.ResponseStream;
            while (await responseStream.MoveNext(cancellation) && !cancellation.IsCancellationRequested)
            {
                yield return responseStream.Current.Data.Memory;
            }

            await Task.WhenAll(pushTask);
        }
    }
}