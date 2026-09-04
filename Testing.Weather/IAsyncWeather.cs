using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public interface IAsyncWeather : IAsyncDisposable
{
    ValueTask<WeatherData> GetWeather(int rngSeed, CancellationToken cancellation = default);

    IAsyncEnumerable<WeatherData> GetForecast(int rngSeed, int count, CancellationToken cancellation = default);
}
