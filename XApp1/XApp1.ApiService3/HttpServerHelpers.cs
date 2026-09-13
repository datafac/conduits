
using DataFac.Conduits;
using DataFac.Conduits.HttpCommon;

namespace XApp1.ApiService3;

public static class HttpServerHelpers
{
    public static void MapConduitEndpoints(this IEndpointRouteBuilder app, ProtocolServer protocolServer)
    {
        app.MapPost(EndpointPath.UnaryRequest, async (HttpContext httpContext, JsonRequest jsonRequest) =>
        {
            NetRequest netRequest = new NetRequest(new ReadOnlyMemory<byte>(jsonRequest.Payload));
            DateTime? deadlineUtc = jsonRequest.DeadlineUtc.HasValue ? new DateTime(jsonRequest.DeadlineUtc.Value, DateTimeKind.Utc) : null;
            var netResponse = await protocolServer.UnaryRequest(netRequest, deadlineUtc);
            return new JsonResponse() { ControlCode = (int)netResponse.Control, Payload = netResponse.Payload.ToArray() }; // todo alloc!
        });

    }
}
