
using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using Nerdbank.MessagePack;
using System.Text.Json;
using Testing.Weather;

namespace XApp1.ApiService3;

public class Program
{
    private static readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private static string GetServiceAddress(string variableName)
    {
        return Environment.GetEnvironmentVariable(variableName)
            ?? throw new InvalidOperationException($"Environment variable '{variableName}' is not set or invalid.");
    }

    private static readonly string _weatherSvcAddress = GetServiceAddress("GRPCSERVICE2_HTTPS");
    private static readonly WeatherClient _weatherSvc = new WeatherClient(new ProtocolClient(new GrpcConduitClient(_weatherSvcAddress)));

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapPost("/weatherforecast", async (HttpContext httpContext, JsonMessage jsonRequest) =>
        {
            RequestBase? requestBase = _serializer.Deserialize<RequestBase>(jsonRequest?.Payload ?? Array.Empty<byte>());
            switch(requestBase)
            {
                case GetForecastRequest fr:
                    WeatherData[] results = await _weatherSvc.GetForecast(fr.RngSeed, fr.Count).ToArrayAsync();
                    BatchResult batch = new BatchResult() { Results = results };
                    return new JsonMessage { Payload = _serializer.Serialize<ResultBase>(batch) };
                default:
                    ErrorResult error = new ErrorResult() { Code = ErrorCode.UnsupportedRequestType, Message = $"Request type '{requestBase?.GetType().Name}' is not supported." };
                    return new JsonMessage { Payload = _serializer.Serialize<ResultBase>(new BatchResult() { Results = new ResultBase[] { error } }) };
            }
        })
        .WithName("PostWeatherForecast");

        app.Run();
    }
}
