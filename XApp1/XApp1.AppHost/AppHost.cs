var builder = DistributedApplication.CreateBuilder(args);

var grpcService1 = builder.AddProject<Projects.XApp1_GrpcService1>("grpcservice1");

var grpcService2 = builder.AddProject<Projects.XApp1_GrpcService2>("grpcservice2");

var apiService3 = builder.AddProject<Projects.XApp1_ApiService3>("apiservice3")
    .WithHttpHealthCheck("/health")
    .WithReference(grpcService1)
    .WithReference(grpcService2)
    .WaitFor(grpcService1)
    .WaitFor(grpcService2)
    ;

builder.AddProject<Projects.XApp1_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService3)
    .WaitFor(apiService3)
    ;

builder.Build().Run();
