
using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using DataFac.Conduits.HttpCommon;

namespace XApp1.ApiService3;

public class Program
{
    private static string GetServiceAddress(string variableName)
    {
        return Environment.GetEnvironmentVariable(variableName)
            ?? throw new InvalidOperationException($"Environment variable '{variableName}' is not set or invalid.");
    }

    private static readonly string _weatherSvcAddress = GetServiceAddress("GRPCSERVICE2_HTTPS");
    private static readonly ProtocolClient _protocolClient = new ProtocolClient(new GrpcConduitClient(_weatherSvcAddress));
    private static readonly ProtocolServer _protocolServer = new ProtocolServer(null, _protocolClient);

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

        app.MapPost(EndpointPath.UnaryRequest, async (HttpContext httpContext, JsonRequest jsonRequest) =>
        {
            NetRequest netRequest = new NetRequest(new ReadOnlyMemory<byte>(jsonRequest.Payload));
            DateTime? deadlineUtc = jsonRequest.DeadlineUtc.HasValue ? new DateTime(jsonRequest.DeadlineUtc.Value, DateTimeKind.Utc) : null;
            var netResponse = await _protocolServer.UnaryRequest(netRequest, deadlineUtc);
            return new JsonResponse() { ControlCode = (int)netResponse.Control, Payload = netResponse.Payload.ToArray() }; // todo alloc!
        });

        app.Run();
    }
}
