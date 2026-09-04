using DataFac.Conduits.GrpcCommon;
using Google.Protobuf;

namespace DataFac.Conduits.GrpcClient;

internal static class GrpcPayloadExtensions
{
    public static GrpcPayload ToGrpcPayload(this NetRequest request)
    {
        return new GrpcPayload()
        {
            Code = (int)request.Control,
            Data = UnsafeByteOperations.UnsafeWrap(request.Payload)
        };
    }
    public static NetResponse ToConduitResponse(this GrpcPayload request)
    {
        return new NetResponse((ControlCode)request.Code, request.Data.Memory);
    }
}
