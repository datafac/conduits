using Google.Protobuf;
using DataFac.Conduits.GrpcCommon;

namespace DataFac.Conduits.GrpcServer;

internal static class GrpcPayloadExtensions
{
    public static GrpcPayload ToGrpcPayload(this NetResponse response)
    {
        return new GrpcPayload()
        {
            Code = (int)response.Control,
            Data = UnsafeByteOperations.UnsafeWrap(response.Payload)
        };
    }
    public static NetRequest ToConduitRequest(this GrpcPayload request)
    {
        return new NetRequest((ControlCode)request.Code, request.Data.Memory);
    }
}