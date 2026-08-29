using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.HttpClient;

public class HttpConduitClient : IConduitClient
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly bool _httpClientOwned = false;
    private readonly SwaggerClient _swagClient;

    public TimeProvider TimeProvider => TimeProvider.System;

    public HttpConduitClient(System.Net.Http.HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _httpClientOwned = false;
        _swagClient = new SwaggerClient(baseUrl, _httpClient);
    }

    public HttpConduitClient(string baseUrl)
    {
        _httpClient = new System.Net.Http.HttpClient();
        _httpClientOwned = true;
        _swagClient = new SwaggerClient(baseUrl, _httpClient);
    }

    private volatile bool _disposed = false;
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_httpClientOwned)
        {
            _httpClient.Dispose();
        }
    }

    public async ValueTask<ConduitResponse> UnaryRequest(ConduitRequest request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var outgoing = request.ToJsonPayload(deadlineUtc.HasValue ? deadlineUtc.Value.Ticks : null);
        var incoming = await _swagClient.NoStreamAsync(outgoing, cancellation);
        return new ConduitResponse(incoming.ToPayload());
    }

    public async IAsyncEnumerable<ConduitResponse> ServerStream(ConduitRequest request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var outgoing = request.Payload.ToUserData(deadlineUtc.HasValue ? deadlineUtc.Value.Ticks : null);
        foreach (var incoming in await _swagClient.StreamDnAsync(outgoing, cancellation))
        {
            yield return new ConduitResponse(incoming.ToPayload());
            if (!cancellation.IsCancellationRequested)
            {
                throw new OperationCanceledException("Deadline exceeded");
            }
        }
    }

    public ValueTask<ConduitResponse> ClientStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotSupportedException();
    }

    public IAsyncEnumerable<ConduitResponse> DuplexStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotSupportedException();
    }
}
