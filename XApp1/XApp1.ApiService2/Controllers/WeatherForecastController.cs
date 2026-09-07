using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Nerdbank.MessagePack;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

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
        List<WeatherData> results = new List<WeatherData>();
        await foreach (var wd in _weatherSvc.GetForecast(0, 7))
        {
            results.Add(wd);
        }

        BatchResult batch = new BatchResult() { Results = results.ToArray() };
        byte[] payload = _serializer.Serialize<ResultBase>(batch);
        var message = new JsonMessage { Payload = payload };
        return message;
    }
}