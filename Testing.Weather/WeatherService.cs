using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public class WeatherService : IAsyncWeather
{
    private static readonly string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public async ValueTask DisposeAsync() { }

    private IEnumerable<WeatherData> GenerateWeather(int rngSeed, int count)
    {
        var rng = rngSeed == 0 ? new Random(Environment.TickCount) : new Random(rngSeed);
        for (var i = 0; i < count; i++)
        {
            yield return new WeatherData()
            {
                DateTimeUtc = DateTime.UtcNow.AddDays(i).Ticks,
                TemperatureC = rng.Next(-20, 55),
                Summary = summaries[rng.Next(summaries.Length)]
            };
        }
    }

    public async ValueTask<WeatherForecast> GetWeatherForecast(int rngSeed, int count, CancellationToken cancellation = default)
    {
        return new WeatherForecast()
        {
            Batch = GenerateWeather(rngSeed, count).ToArray()
        };
    }
}
