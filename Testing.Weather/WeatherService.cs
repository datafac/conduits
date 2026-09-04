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

    public async ValueTask<WeatherData> GetWeather(CancellationToken cancellation = default)
    {
        var rng = new Random(Environment.TickCount);
        return new WeatherData()
        {
            DateTimeUtc = DateTime.UtcNow.Ticks,
            TemperatureC = rng.Next(-20, 55),
            Summary = summaries[rng.Next(summaries.Length)]
        };
    }

    public async IAsyncEnumerable<WeatherData> GetForecast(int count, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var rng = new Random(Environment.TickCount);
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
