
using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using Testing.Weather;

namespace XApp1.ApiService2;

public class Program
{
    private static string GetServiceAddress(string variableName)
    {
        return Environment.GetEnvironmentVariable(variableName)
            ?? throw new InvalidOperationException($"Environment variable '{variableName}' is not set or invalid.");
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.

        builder.Services.AddControllers();
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

        app.MapControllers();

        var weatherSvc = new WeatherClient(new ProtocolClient(new GrpcConduitClient(GetServiceAddress("GRPCSERVICE2_HTTPS"))));
        try
        {
            var weather = await weatherSvc.GetWeather(12345);
        }
        finally
        {
            await weatherSvc.DisposeAsync();
        }

        app.Run();
    }
}
