using Shouldly;
using System;
using System.Threading.Tasks;
using Testing.Calculator;
using XGrpcClient;
using XGrpcServer;
using XGrpcShared;

namespace XGrpcTests;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

public class GrpcUnaryRequestTests
{
    private const string host = "localhost";

    [Fact]
    public async Task Multiply()
    {
        await using CalculatorServer server = new CalculatorServer();
        await using CalculatorClient client = new CalculatorClient(host, server.BoundPort);

        var result = await client.DoBinOp(3, BinOp.Multiply, 4);
        result.ShouldBe(12);
    }

    [Fact]
    public async Task Divide()
    {
        await using CalculatorServer server = new CalculatorServer();
        await using CalculatorClient client = new CalculatorClient(host, server.BoundPort);

        var result = await client.DoBinOp(12, BinOp.Divide, 4);
        result.ShouldBe(3);
    }

    [Fact]
    public async Task DivideByZero()
    {
        await using CalculatorServer server = new CalculatorServer();
        await using CalculatorClient client = new CalculatorClient(host, server.BoundPort);

        var result = await client.DoBinOp(12, BinOp.Divide, 0);
        result.ShouldBe(double.PositiveInfinity);
    }

    [Fact]
    public async Task DivideByZero2()
    {
        await using CalculatorServer server = new CalculatorServer();
        await using CalculatorClient client = new CalculatorClient(host, server.BoundPort);

        var result = await client.DoBinOp(0, BinOp.Divide, 0);
        result.ShouldBe(double.NaN);
    }

    [Fact]
    public async Task Add()
    {
        await using CalculatorServer server = new CalculatorServer();
        await using CalculatorClient client = new CalculatorClient(host, server.BoundPort);

        var result = await client.DoBinOp(4, BinOp.Add, 3);
        result.ShouldBe(7);
    }

    [Fact]
    public async Task Subtract()
    {
        await using CalculatorServer server = new CalculatorServer();
        await using CalculatorClient client = new CalculatorClient(host, server.BoundPort);

        var result = await client.DoBinOp(4, BinOp.Subtract, 3);
        result.ShouldBe(1);
    }
}
