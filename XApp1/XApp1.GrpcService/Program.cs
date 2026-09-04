using DataFac.Conduits;
using DataFac.Conduits.GrpcServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testing.Calculator;

namespace XApp1.GrpcService1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddGrpc();

        builder.Services.AddSingleton<IUserChannel>(sp => new CalculatorServer(new Calculator()));
        builder.Services.AddSingleton<INetChannel>(sp => new ProtocolServer(null, sp.GetRequiredService<IUserChannel>()));

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<GrpcService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}
