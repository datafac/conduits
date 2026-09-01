var builder = DistributedApplication.CreateBuilder(args);

var grpcService = builder.AddProject<Projects.XApp1_GrpcService>("grpcservice");

var apiService = builder.AddProject<Projects.XApp1_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(grpcService)
    .WaitFor(grpcService);

builder.AddProject<Projects.XApp1_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
