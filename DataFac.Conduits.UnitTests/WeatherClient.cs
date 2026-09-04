using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.UnitTests;

public class WeatherClient : IWeatherService, IAsyncDisposable
{
    private readonly IUserChannel _client;
    private readonly bool Owned;

    public WeatherClient(IUserChannel client, bool owned = false)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        Owned = owned;
    }

    public async ValueTask DisposeAsync()
    {
        if (Owned && _client is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }

    public async ValueTask<WeatherData> GetWeather(string location, CancellationToken token)
    {
        var request = new WeatherData(WeatherTag.GetWeatherData, location);
        var reply = await _client.UnaryRequest(new UserRequest(request.ToMemory()), token);
        return WeatherData.FromSpan(reply.Payload.Span);
    }

    public async IAsyncEnumerable<WeatherData> GetWeatherStream(string location, [EnumeratorCancellation] CancellationToken token)
    {
        var request = new WeatherData(WeatherTag.StreamDn_WeatherFeed, location);
        var payload = request.ToMemory();
        await foreach (var response in _client.ServerStream(new UserRequest(request.ToMemory()), token))
        {
            var result = WeatherData.FromSpan(response.Payload.Span);
            if (result is not null)
                yield return result;
        }
    }

    public async ValueTask UpdateWeather(WeatherData request, CancellationToken token)
    {

        var _ = await _client.UnaryRequest(new UserRequest(request.ToMemory()), token);
    }
}
