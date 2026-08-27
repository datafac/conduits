using DataFac.Conduits.GrpcCommon;
using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.GrpcClient;

public class GrpcConduitClient : IConduitClient
{
    private readonly GrpcChannel _channel;

    public TimeProvider TimeProvider => TimeProvider.System;

    public GrpcConduitClient(Uri address)
    {
        _channel = GrpcChannel.ForAddress(address);
    }

    private volatile bool _disposed = false;

    public async ValueTask DisposeAsync()
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

    public async ValueTask<ConduitResponse> SimpleUnaryCall(ConduitRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        var client = new GrpcService.GrpcServiceClient(_channel);
        var incoming = await client.NoStreamAsync(new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(request.Payload) }, null, deadlineUtc, cancellation);
        return new ConduitResponse(incoming.Data.Memory);
    }

    public async IAsyncEnumerable<ConduitResponse> ServerStream(ConduitRequest request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        var client = new GrpcService.GrpcServiceClient(_channel);
        var call = client.StreamDn(new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(request.Payload) }, null, deadlineUtc, cancellation);

        var responseStream = call.ResponseStream;
        while (await responseStream.MoveNext(cancellation) && !cancellation.IsCancellationRequested)
        {
            yield return new ConduitResponse(responseStream.Current.Data.Memory);
        }
    }

    public async ValueTask<ConduitResponse> ClientStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        var client = new GrpcService.GrpcServiceClient(_channel);
        using var call = client.StreamUp(deadline: deadlineUtc, cancellationToken: cancellation);
        var pushTask = Task.Run(async () =>
        {
            var requestStream = call.RequestStream;
            await foreach (var request in requests)
            {
                await requestStream.WriteAsync(new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(request.Payload) });
            }

            await requestStream.CompleteAsync();
        });
        await Task.WhenAll(pushTask);
        var incoming = await call.ResponseAsync;
        return new ConduitResponse(incoming.Data.Memory);
    }

    public async IAsyncEnumerable<ConduitResponse> DuplexStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        var client = new GrpcService.GrpcServiceClient(_channel);
        using var call = client.BiStream(cancellationToken: cancellation, deadline: deadlineUtc);

        var pushTask = Task.Run(async () =>
        {
            var requestStream = call.RequestStream;
            await foreach (var request in requests)
            {
                await requestStream.WriteAsync(new GrpcPayload() { Data = UnsafeByteOperations.UnsafeWrap(request.Payload) });
            }

            await requestStream.CompleteAsync();
        });

        var responseStream = call.ResponseStream;
        while (await responseStream.MoveNext(cancellation) && !cancellation.IsCancellationRequested)
        {
            yield return new ConduitResponse(responseStream.Current.Data.Memory);
        }

        await Task.WhenAll(pushTask);
    }
}
