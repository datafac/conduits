using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using Microsoft.AspNetCore.Mvc;
using Testing.Weather;

namespace XApp1.ApiService2.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase, IAsyncDisposable
{
    private static string GetServiceAddress(string variableName)
    {
        return Environment.GetEnvironmentVariable(variableName)
            ?? throw new InvalidOperationException($"Environment variable '{variableName}' is not set or invalid.");
    }

    private readonly string _weatherSvcAddress = GetServiceAddress("GRPCSERVICE2_HTTPS");
    private readonly WeatherClient _weatherSvc;

    public WeatherForecastController()
    {
        _weatherSvc = new WeatherClient(new ProtocolClient(new GrpcConduitClient(_weatherSvcAddress)));
    }

    public async ValueTask DisposeAsync()
    {
        await _weatherSvc.DisposeAsync();
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async IAsyncEnumerable<WeatherForecast> GetAll()
    {
        var weather = await _weatherSvc.GetWeather(12345);

        await foreach (var wd in _weatherSvc.GetForecast(0, 7))
        {
            yield return new WeatherForecast
            {
                Date = DateOnly.FromDateTime(new DateTime(wd.DateTimeUtc, DateTimeKind.Utc)),
                TemperatureC = wd.TemperatureC,
                Summary = wd.Summary
            };
        }
    }
}
