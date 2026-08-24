using Shouldly;
using System;
using System.Threading.Tasks;
using Testing.Calculator;
using Testing.Calculator.Server;
using XGrpcClient;
using XGrpcServer;

namespace XGrpcTests;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

public class GrpcUnaryRequestTests
{
    private const string host = "localhost";

    [Fact]
    public async Task Multiply()
    {
        await using var server = new ProtobufGrpcServer(new RequestHandler(new Calculator()));
        await using var client = new ProtobufGrpcClient(host, server.BoundPort);
        await using CalculatorClient calculator = new CalculatorClient(client);

        var result = await calculator.DoBinOp(3, BinOp.Multiply, 4);
        result.ShouldBe(12);
    }

    [Fact]
    public async Task Divide()
    {
        await using var server = new ProtobufGrpcServer(new RequestHandler(new Calculator()));
        await using var client = new ProtobufGrpcClient(host, server.BoundPort);
        await using CalculatorClient calculator = new CalculatorClient(client);

        var result = await calculator.DoBinOp(12, BinOp.Divide, 4);
        result.ShouldBe(3);
    }

    [Fact]
    public async Task DivideByZero()
    {
        await using var server = new ProtobufGrpcServer(new RequestHandler(new Calculator()));
        await using var client = new ProtobufGrpcClient(host, server.BoundPort);
        await using CalculatorClient calculator = new CalculatorClient(client);

        var result = await calculator.DoBinOp(12, BinOp.Divide, 0);
        result.ShouldBe(double.PositiveInfinity);
    }

    [Fact]
    public async Task DivideByZero2()
    {
        await using var server = new ProtobufGrpcServer(new RequestHandler(new Calculator()));
        await using var client = new ProtobufGrpcClient(host, server.BoundPort);
        await using CalculatorClient calculator = new CalculatorClient(client);

        var result = await calculator.DoBinOp(0, BinOp.Divide, 0);
        result.ShouldBe(double.NaN);
    }

    [Fact]
    public async Task Add()
    {
        await using var server = new ProtobufGrpcServer(new RequestHandler(new Calculator()));
        await using var client = new ProtobufGrpcClient(host, server.BoundPort);
        await using CalculatorClient calculator = new CalculatorClient(client);

        var result = await calculator.DoBinOp(4, BinOp.Add, 3);
        result.ShouldBe(7);
    }

    [Fact]
    public async Task Subtract()
    {
        await using var server = new ProtobufGrpcServer(new RequestHandler(new Calculator()));
        await using var client = new ProtobufGrpcClient(host, server.BoundPort);
        await using CalculatorClient calculator = new CalculatorClient(client);

        var result = await calculator.DoBinOp(4, BinOp.Subtract, 3);
        result.ShouldBe(1);
    }
}
