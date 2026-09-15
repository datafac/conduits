using DataFac.Conduits.HttpCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace DataFac.Conduits.HttpServer
{
    public static class HttpServerHelpers
    {
        public static void MapConduitEndpoints(this IEndpointRouteBuilder app, ConduitServer protocolServer)
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
}
