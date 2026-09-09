using System;

namespace DataFac.Conduits.HttpCommon;

public class JsonRequest
{
    public long? DeadlineUtc { get; set; }
    public byte[]? Payload { get; set; }
}
public class JsonResponse
{
    public int ControlCode { get; set; }
    public byte[]? Payload { get; set; }
}

public static class EndpointPath
{
    public const string UnaryRequest = "/conduitunaryrequest";
    public const string ClientStream = "/conduitclientstream";
    public const string ServerStream = "/conduitserverstream";
    public const string DuplexStream = "/conduitduplexstream";
}