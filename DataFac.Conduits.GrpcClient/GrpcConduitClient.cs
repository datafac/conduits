using DataFac.Conduits.GrpcCommon;
using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.GrpcClient;

internal static class GrpcPayloadExtensions
{
    // todo remove
    public static GrpcPayload ToGrpcPayload(this ConduitResponse response)
    {
        return new GrpcPayload()
        {
            Code = (int)response.Control,
            Dataqqq = UnsafeByteOperations.UnsafeWrap(response.Payloadqqq)
        };
    }

    public static GrpcPayload ToGrpcPayload(this ConduitRequest request)
    {
        return new GrpcPayload()
        {
            Code = (int)request.Control,
            Dataqqq = UnsafeByteOperations.UnsafeWrap(request.Payloadqqq)
        };
    }

    // todo remove
    public static ConduitRequest ToConduitRequest(this GrpcPayload request)
    {
        return new ConduitRequest((ControlCode)request.Code, request.Dataqqq.Memory);
    }

    public static ConduitResponse ToConduitResponse(this GrpcPayload request)
    {
        return new ConduitResponse((ControlCode)request.Code, request.Dataqqq.Memory);
    }
}

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

    public async ValueTask<ConduitResponse> UnaryRequest(ConduitRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        var client = new GrpcService.GrpcServiceClient(_channel);
        var incoming = await client.NoStreamAsync(request.ToGrpcPayload(), null, deadlineUtc, cancellation);
        return incoming.ToConduitResponse();
    }

    public async IAsyncEnumerable<ConduitResponse> ServerStream(ConduitRequest request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        CheckNotDisposed();
        var client = new GrpcService.GrpcServiceClient(_channel);
        var call = client.StreamDn(request.ToGrpcPayload(), null, deadlineUtc, cancellation);

        var responseStream = call.ResponseStream;
        while (await responseStream.MoveNext(cancellation) && !cancellation.IsCancellationRequested)
        {
            yield return responseStream.Current.ToConduitResponse();
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
                await requestStream.WriteAsync(request.ToGrpcPayload());
            }

            await requestStream.CompleteAsync();
        });
        await Task.WhenAll(pushTask);
        var incoming = await call.ResponseAsync;
        return incoming.ToConduitResponse();
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
