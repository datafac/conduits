var builder = DistributedApplication.CreateBuilder(args);

// todo grpc calc server
//xxx;

var apiService = builder.AddProject<Projects.XApp1_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.XApp1_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
