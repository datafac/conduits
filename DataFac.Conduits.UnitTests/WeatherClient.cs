using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.UnitTests
{
    public class WeatherClient : IWeatherService, IDisposable
    {
        private readonly IConduitClient _client;
        private readonly bool Owned;

        public WeatherClient(IConduitClient client, bool owned = false)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Owned = owned;
        }

        public void Dispose()
        {
            if (Owned && _client is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public async ValueTask<WeatherData> GetWeather(string location, CancellationToken token)
        {
            var request = new WeatherData(WeatherTag.GetWeatherData, location);
            var payload = request.ToMemory();
            var reply = await _client.SimpleUnaryCall(payload, DateTime.UtcNow.AddSeconds(30), token);
            return WeatherData.FromSpan(reply.Span);
        }

        public async IAsyncEnumerable<WeatherData> GetWeatherStream(string location, [EnumeratorCancellation] CancellationToken token)
        {
            var request = new WeatherData(WeatherTag.StreamDn_WeatherFeed, location);
            var payload = request.ToMemory();
            await foreach (var response in _client.ServerStream(payload, DateTime.UtcNow.AddSeconds(30), token))
            {
                var result = WeatherData.FromSpan(response.Span);
                if (result is not null)
                    yield return result;
            }
        }

        public async ValueTask UpdateWeather(WeatherData request, CancellationToken token)
        {
            var payload = request.ToMemory();
            var _ = await _client.SimpleUnaryCall(payload, DateTime.UtcNow.AddSeconds(30), token);
        }

    }
}