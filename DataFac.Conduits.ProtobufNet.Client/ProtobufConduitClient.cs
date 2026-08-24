using DataFac.Conduits.ProtobufNetCommon;
using Grpc.Core;
using ProtoBuf;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.ProtobufNetClient;

public class ProtobufConduitClient : IConduitClient
{
    private readonly Channel _channel;
    private readonly IProtobufNetContract _contract;

    public ProtobufConduitClient(string server, int port)
    {
        _channel = new Channel(server, port, ChannelCredentials.Insecure);
        _contract = _channel.CreateGrpcService<IProtobufNetContract>();
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.ShutdownAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    public ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var callOptions = new CallOptions(null, deadlineUtc, cancellation);
        RequestBlob requestBlob = new RequestBlob() { Blob = request.ToArray() }; // todo alloc
        await foreach (var resultBlob in _contract.ServerStream(requestBlob, new CallContext(callOptions)))
        {
            yield return resultBlob.Blob;
        }
    }

    public async ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var callOptions = new CallOptions(null, deadlineUtc, cancellation);
        RequestBlob requestBlob = new RequestBlob() { Blob = request.ToArray() }; // todo alloc
        var resultBlob = await _contract.UnaryRequest(requestBlob, new CallContext(callOptions));
        return resultBlob.Blob;
    }
}
