using Google.Protobuf;
using DataFac.Conduits.GrpcCommon;
using System;

namespace DataFac.Conduits.GrpcServer;

internal static class GrpcPayloadExtensions
{
    public static GrpcPayload ToGrpcPayload(this ConduitResponse response)
    {
        return new GrpcPayload()
        {
            Code = (int)response.Control,
            Dataqqq = UnsafeByteOperations.UnsafeWrap(response.Payloadqqq)
        };
    }

    // todo remove
    public static GrpcPayload ToGrpcPayload(this ConduitRequest request)
    {
        return new GrpcPayload()
        {
            Code = (int)request.Control,
            Dataqqq = UnsafeByteOperations.UnsafeWrap(request.Payloadqqq)
        };
    }

    public static ConduitRequest ToConduitRequest(this GrpcPayload request)
    {
        return new ConduitRequest((ControlCode)request.Code, request.Dataqqq.Memory);
    }

    // todo remove
    public static ConduitResponse ToConduitResponse(this GrpcPayload request)
    {
        return new ConduitResponse((ControlCode)request.Code, request.Dataqqq.Memory);
    }
}