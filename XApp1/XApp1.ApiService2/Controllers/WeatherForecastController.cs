using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using Microsoft.AspNetCore.Mvc;
using Nerdbank.MessagePack;
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

    private static readonly string _weatherSvcAddress = GetServiceAddress("GRPCSERVICE2_HTTPS");
    private readonly WeatherClient _weatherSvc;

    private static readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    public WeatherForecastController()
    {
        _weatherSvc = new WeatherClient(new ProtocolClient(new GrpcConduitClient(_weatherSvcAddress)));
    }

    public async ValueTask DisposeAsync()
    {
        await _weatherSvc.DisposeAsync();
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<JsonMessage> GetAll()
    {
        WeatherData[] results = await _weatherSvc.GetForecast(0, 7).ToArrayAsync();
        BatchResult batch = new BatchResult() { Results = results };
        byte[] payload = _serializer.Serialize<ResultBase>(batch);
        var message = new JsonMessage { Payload = payload };
        return message;
    }
}