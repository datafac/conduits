using DataFac.Conduits.GrpcCommon;
using Google.Protobuf;

namespace DataFac.Conduits.GrpcClient;

internal static class GrpcPayloadExtensions
{
    public static GrpcPayload ToGrpcPayload(this ConduitRequest request)
    {
        return new GrpcPayload()
        {
            Code = (int)request.Control,
            Data = UnsafeByteOperations.UnsafeWrap(request.Payload)
        };
    }
    public static ConduitResponse ToConduitResponse(this GrpcPayload request)
    {
        return new ConduitResponse((ControlCode)request.Code, request.Data.Memory);
    }
}
