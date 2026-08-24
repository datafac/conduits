using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

namespace XGrpcShared
{
    [Service]
    public interface IConduit
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
}
