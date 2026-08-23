using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DataFac.Conduits.UnitTests
{
    public class WeatherService : IWeatherService, IDisposable
    {
        private readonly ConcurrentDictionary<string, WeatherData> _weatherDb = new ConcurrentDictionary<string, WeatherData>();
        private readonly ConcurrentDictionary<string, Subject<WeatherData>> _weatherHub = new ConcurrentDictionary<string, Subject<WeatherData>>();


        public void Dispose()
        {
            var subjects = _weatherHub.Values.ToArray();
            foreach (var subject in subjects)
            {
                subject.Dispose();
            }
        }


        public ValueTask<WeatherData> GetWeather(string location, CancellationToken token)
        {
            // query db
            if (_weatherDb.TryGetValue(location, out var weather))
                return new ValueTask<WeatherData>(weather);
            else
                return new ValueTask<WeatherData>(new WeatherData(WeatherTag.NotFound, location));
        }

        public async IAsyncEnumerable<WeatherData> GetWeatherStream(string location, [EnumeratorCancellation] CancellationToken token)
        {
            var options = new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true };
            var channel = Channel.CreateUnbounded<WeatherData>(options);
            var writer = channel.Writer;

            // return current value
            if (_weatherDb.TryGetValue(location, out var weather))
            {
                await writer.WriteAsync(weather);
            }

            // subscribe to updates
            _weatherHub
                .GetOrAdd(location, (l) => new Subject<WeatherData>())
                .Subscribe(
                    (w) => { writer.TryWrite(w); },
                    (e) => { writer.TryComplete(e); },
                    () => { writer.TryComplete(); },
                    token);

            var reader = channel.Reader;
            await foreach (var response in reader.ReadAllAsync(token))
            {
                yield return response;
            }
        }

        public ValueTask UpdateWeather(WeatherData weather, CancellationToken token)
        {
            // update db
            _weatherDb.AddOrUpdate(weather.Location,
                weather,
                (l, old) => weather);

            // publish
            _weatherHub
                .GetOrAdd(weather.Location, (l) => new Subject<WeatherData>())
                .OnNext(weather);

            return new ValueTask();
        }

    }
}