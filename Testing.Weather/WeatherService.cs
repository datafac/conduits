using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public class WeatherService : IAsyncWeather
{
    private static readonly string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public async ValueTask DisposeAsync() { }

    public async ValueTask<WeatherData> GetWeather(int rngSeed, CancellationToken cancellation = default)
    {
        var rng = rngSeed == 0 ? new Random(Environment.TickCount) : new Random(rngSeed);
        return new WeatherData()
        {
            DateTimeUtc = DateTime.UtcNow.Ticks,
            TemperatureC = rng.Next(-20, 55),
            Summary = summaries[rng.Next(summaries.Length)]
        };
    }

    public async IAsyncEnumerable<WeatherData> GetForecast(int rngSeed, int count, [EnumeratorCancellation] CancellationToken cancellation = default)
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

}
