using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using Microsoft.AspNetCore.Mvc;
using Testing.Weather;

namespace XApp1.ApiService2.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static string GetServiceAddress(string variableName)
    {
        return Environment.GetEnvironmentVariable(variableName)
            ?? throw new InvalidOperationException($"Environment variable '{variableName}' is not set or invalid.");
    }

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> GetAll()
    {
        //await using var weatherSvc = new WeatherClient(new ProtocolClient(new GrpcConduitClient(GetServiceAddress("GRPCSERVICE2_HTTPS"))));
        //var weather = await weatherSvc.GetWeather(12345);

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
