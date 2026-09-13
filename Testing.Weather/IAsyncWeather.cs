using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public interface IAsyncWeather : IAsyncDisposable
{
    ValueTask<WeatherForecast> GetWeatherForecast(int rngSeed, int count, CancellationToken cancellation = default);
}
