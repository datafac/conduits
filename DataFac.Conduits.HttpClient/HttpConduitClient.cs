using DataFac.Conduits.HttpCommon;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.HttpClient;

public sealed class HttpConduitClient : INetConduit
{
    private readonly System.Net.Http.HttpClient _httpClient;
    public HttpConduitClient(System.Net.Http.HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static ReadOnlyMemory<byte> EncodeErrorMessage(string message)
    {
        if (message.Length == 0) return ReadOnlyMemory<byte>.Empty;
        return Encoding.UTF8.GetBytes(message);
    }

    public async ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadline, CancellationToken cancellation = default)
    {
        var jsonRequest = new JsonRequest { DeadlineUtc = deadline?.Ticks, Payload = request.Payload.ToArray() };
        var httpResponse = await _httpClient.PostAsJsonAsync<JsonRequest>(EndpointPath.UnaryRequest, jsonRequest, cancellation);
        JsonResponse? jsonResponse = await httpResponse.Content.ReadFromJsonAsync<JsonResponse>(cancellation);
        if (jsonResponse is null) return new NetResponse(ControlCode.InvalidData, EncodeErrorMessage("Failed to deserialize response."));
        ReadOnlyMemory<byte> payload = jsonResponse.Payload ?? ReadOnlyMemory<byte>.Empty;
        return new NetResponse((ControlCode)jsonResponse.ControlCode, payload);
    }

    public async ValueTask<NetResponse> ClientStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadline, CancellationToken cancellation = default)
    {
        return new NetResponse(ControlCode.InvalidOp, EncodeErrorMessage("Client streaming is not supported."));
    }

    public async IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadline, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        yield return new NetResponse(ControlCode.InvalidOp, EncodeErrorMessage("Duplex streaming is not supported."));
    }

    public async IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadline, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        yield return new NetResponse(ControlCode.InvalidOp, EncodeErrorMessage("Server streaming is not supported."));
    }
}

