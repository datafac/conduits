using DataFac.Conduits.GrpcCommon;
using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.GrpcClient;

public class GrpcConduitClient : INetChannel, IAsyncDisposable
{
    private readonly GrpcChannel _channel;
    private readonly GrpcService.GrpcServiceClient _client;

    public TimeProvider TimeProvider => TimeProvider.System;

    public GrpcConduitClient(string address)
    {
        _channel = GrpcChannel.ForAddress(address);
        _client = new GrpcService.GrpcServiceClient(_channel);
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _channel.Dispose();
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

    public async ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        var incoming = await _client.NoStreamAsync(request.ToGrpcPayload(), null, deadlineUtc, cancellation);
        return incoming.ToConduitResponse();
    }

    public async IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        var call = _client.StreamDn(request.ToGrpcPayload(), null, deadlineUtc, cancellation);

        var responseStream = call.ResponseStream;
        while (await responseStream.MoveNext(cancellation) && !cancellation.IsCancellationRequested)
        {
            yield return responseStream.Current.ToConduitResponse();
        }
    }

    public async ValueTask<NetResponse> ClientStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        using var call = _client.StreamUp(deadline: deadlineUtc, cancellationToken: cancellation);
        var pushTask = Task.Run(async () =>
        {
            var requestStream = call.RequestStream;
            await foreach (var request in requests)
            {
                await requestStream.WriteAsync(request.ToGrpcPayload());
            }

            await requestStream.CompleteAsync();
        });
        await Task.WhenAll(pushTask);
        var incoming = await call.ResponseAsync;
        return incoming.ToConduitResponse();
    }

    public async IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        using var call = _client.BiStream(cancellationToken: cancellation, deadline: deadlineUtc);

        var pushTask = Task.Run(async () =>
        {
            var requestStream = call.RequestStream;
            await foreach (var request in requests)
            {
                await requestStream.WriteAsync(request.ToGrpcPayload());
            }

            await requestStream.CompleteAsync();
        });

        var responseStream = call.ResponseStream;
        while (await responseStream.MoveNext(cancellation) && !cancellation.IsCancellationRequested)
        {
            yield return responseStream.Current.ToConduitResponse();
        }

        await Task.WhenAll(pushTask);
    }
}
