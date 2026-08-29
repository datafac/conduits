using Google.Protobuf;
using DataFac.Conduits.GrpcCommon;

namespace DataFac.Conduits.GrpcServer;

internal static class GrpcPayloadExtensions
{
    public static GrpcPayload ToGrpcPayload(this ConduitResponse response)
    {
        return new GrpcPayload()
        {
            Code = (int)response.Control,
            Data = UnsafeByteOperations.UnsafeWrap(response.Payload)
        };
    }
    public static ConduitRequest ToConduitRequest(this GrpcPayload request)
    {
        return new ConduitRequest((ControlCode)request.Code, request.Data.Memory);
    }
}