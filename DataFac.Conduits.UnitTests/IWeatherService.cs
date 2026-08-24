using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.UnitTests;

public interface IWeatherService
{
    ValueTask<WeatherData> GetWeather(string location, CancellationToken token);
    IAsyncEnumerable<WeatherData> GetWeatherStream(string locations, CancellationToken token);
    ValueTask UpdateWeather(WeatherData weather, CancellationToken token);
}