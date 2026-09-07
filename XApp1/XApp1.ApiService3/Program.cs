
using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using Nerdbank.MessagePack;
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

        app.MapGet("/weatherforecast", async (HttpContext httpContext) =>
        {
            WeatherData[] results = await _weatherSvc.GetForecast(0, 7).ToArrayAsync();
            BatchResult batch = new BatchResult() { Results = results.ToArray() };
            byte[] payload = _serializer.Serialize<ResultBase>(batch);
            var message = new JsonMessage { Payload = payload };
            return message;
        })
        .WithName("GetWeatherForecast");

        app.Run();
    }
}
