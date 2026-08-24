using ProtoBuf;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataFac.Conduits.ProtobufNet.Common
{
    [Service]
    public interface IProtobufNetConduit
    {
        [Operation]
        ValueTask<ResultBlob> UnaryRequest(RequestBlob request, CallContext context = default);

        [Operation]
        IAsyncEnumerable<ResultBlob> ServerStream(RequestBlob request, CallContext context = default);

        [Operation]
        ValueTask<ResultBlob> ClientStream(IAsyncEnumerable<RequestBlob> requests, CallContext context = default);

        [Operation]
        IAsyncEnumerable<ResultBlob> DuplexStream(IAsyncEnumerable<RequestBlob> requests, CallContext context = default);
    }

    [ProtoContract]
    public sealed class RequestBlob
    {
        [ProtoMember(1)]
        public byte[] Blob { get; set; } = Array.Empty<byte>();
    }
    [ProtoContract]
    public sealed class ResultBlob
    {
        [ProtoMember(1)]
        public byte[] Blob { get; set; } = Array.Empty<byte>();
    }
}
