using DataFac.Conduits;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public interface IAsyncWeather : IAsyncDisposable
{
    ValueTask<AppInfo> GetAppInfo(CancellationToken cancellation = default);
    ValueTask<WeatherForecast> GetWeatherForecast(int rngSeed, int count, CancellationToken cancellation = default);
}
