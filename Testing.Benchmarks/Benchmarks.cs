// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using BenchmarkDotNet.Attributes;
using DataFac.Conduits;
using DataFac.Conduits.GrpcClient;
using DataFac.Conduits.ProtobufNetClient;
using DataFac.Conduits.ProtobufNetServer;
using DataFac.Conduits.Testing;
using Microsoft.VSDiagnostics;
using ProtoBuf.Grpc.Configuration;
using System.Threading.Tasks;
using Testing.Calculator;

namespace Testing.Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private const string host = "localhost";

        private FakeConduitServer fakeServer;
        private CalculatorClient fakeClient;

        //private DataFac.Conduits.GrpcServer.GrpcService grpcServer;
        //private CalculatorClient grpcClient;

        private ProtobufGrpcServer pbufServer;
        private CalculatorClient pbufClient;

        private byte[] data;

        [GlobalSetup]
        public void Setup()
        {
            var timeProvider = new FakeTimeProvider();
            pbufServer = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Testing.Calculator.Calculator())));
            pbufClient = new CalculatorClient(new ProtobufGrpcClient(host, pbufServer.BoundPort));

            //DataFac.Conduits.GrpcServer.GrpcService grpcServer = new DataFac.Conduits.GrpcServer.GrpcService(new ConduitServer(timeProvider, new CalculatorServer(new Testing.Calculator.Calculator())));
            //grpcClient = new CalculatorClient(new GrpcConduitClient(address));

            fakeServer = new FakeConduitServer(new ConduitServer(timeProvider, new CalculatorServer(new Testing.Calculator.Calculator())));
            fakeClient = new CalculatorClient(new FakeConduitClient(fakeServer, timeProvider));
        }

        [GlobalCleanup]

        public async Task CleanupAsync()
        {
            await fakeClient.DisposeAsync();
            await fakeServer.DisposeAsync();

            await pbufClient.DisposeAsync();
            await pbufServer.DisposeAsync();

            //await grpcClient.DisposeAsync();
            //await grpcServer.close
        }

        [Benchmark(Baseline = true)]
        public async ValueTask<double> TestFake()
        {

            double result = await fakeClient.DoBinOp(3, BinOp.Multiply, 4);
            return result;
        }

        [Benchmark]
        public async ValueTask<double> ProtobufNet()
        {

            double result = await pbufClient.DoBinOp(3, BinOp.Multiply, 4);
            return result;
        }

        //[Benchmark]
        //public async ValueTask<double> GoogleGrpc()
        //{
        //    todo double result = await grpcClient.DoBinOp(3, BinOp.Multiply, 4);
        //    return result;
        //}
    }
}
