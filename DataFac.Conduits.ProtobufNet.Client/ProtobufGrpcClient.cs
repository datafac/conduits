using DataFac.Conduits.ProtobufNetCommon;
using Grpc.Core;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.ProtobufNetClient;

internal static class PayloadExtensions
{
    public static RequestBlob ToRequestBlob(this ConduitRequest request)
    {
        return new RequestBlob()
        {
            Control = (int)request.Control,
            Payload = request.Payload.ToArray() // todo alloc!
        };
    }

    public static ConduitResponse ToConduitResponse(this ResultBlob result)
    {
        return new ConduitResponse(
            (ControlCode)result.Control,
            new ReadOnlyMemory<byte>(result.Payload));
    }
}

public class ProtobufGrpcClient : IConduitClient
{
    private readonly Channel _channel;
    private readonly IProtobufNetContract _contract;

    public TimeProvider TimeProvider => TimeProvider.System;

    public ProtobufGrpcClient(string server, int port)
    {
        _channel = new Channel(server, port, ChannelCredentials.Insecure);
        _contract = _channel.CreateGrpcService<IProtobufNetContract>();
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.ShutdownAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    public async ValueTask<ConduitResponse> UnaryRequest(ConduitRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var callOptions = new CallOptions(null, deadlineUtc, cancellation);
        RequestBlob requestBlob = request.ToRequestBlob();
        var resultBlob = await _contract.UnaryRequest(requestBlob, new CallContext(callOptions));
        return resultBlob.ToConduitResponse();
    }

    public async IAsyncEnumerable<ConduitResponse> ServerStream(ConduitRequest request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var callOptions = new CallOptions(null, deadlineUtc, cancellation);
        RequestBlob requestBlob = request.ToRequestBlob();
        await foreach (var resultBlob in _contract.ServerStream(requestBlob, new CallContext(callOptions)))
        {
            yield return resultBlob.ToConduitResponse();
        }
    }

    public ValueTask<ConduitResponse> ClientStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ConduitResponse> DuplexStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }
}