using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.HttpClient;

public class HttpConduitClient : IDisposable, IConduitClient
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly bool _httpClientOwned = false;
    private readonly SwaggerClient _swagClient;

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
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_httpClientOwned)
        {
            _httpClient.Dispose();
        }
    }

    public async ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        var outgoing = request.ToUserData(deadlineUtc.HasValue ? deadlineUtc.Value.Ticks : null);
        var incoming = await _swagClient.NoStreamAsync(outgoing, cancellation);
        return incoming.ToPayload();
    }

    public async IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> request, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var outgoing = request.ToUserData(deadlineUtc.HasValue ? deadlineUtc.Value.Ticks : null);
        foreach (var incoming in await _swagClient.StreamDnAsync(outgoing, cancellation))
        {
            yield return incoming.ToPayload();
            if (!cancellation.IsCancellationRequested)
            {
                throw new OperationCanceledException("Deadline exceeded");
            }
        }
    }

    public ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotSupportedException();
    }

    public IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotSupportedException();
    }
}
