using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public interface IAsyncWeather : IAsyncDisposable
{
    ValueTask<WeatherData> GetWeather(CancellationToken cancellation = default);

    IAsyncEnumerable<WeatherData> GetForecast(int count, CancellationToken cancellation = default);
}
