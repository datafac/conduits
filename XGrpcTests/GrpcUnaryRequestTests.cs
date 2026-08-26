using DataFac.Conduits.ProtobufNetClient;
using DataFac.Conduits.ProtobufNetServer;
using Shouldly;
using System.Threading.Tasks;
using Testing.Calculator;

namespace XGrpcTests;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

public class GrpcUnaryRequestTests
{
    private const string host = "localhost";

    [Fact]
    public async Task Multiply()
    {
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(3, BinOp.Multiply, 4);
        result.ShouldBe(12);
    }

    [Fact]
    public async Task Divide()
    {
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(12, BinOp.Divide, 4);
        result.ShouldBe(3);
    }

    [Fact]
    public async Task DivideByZero()
    {
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(12, BinOp.Divide, 0);
        result.ShouldBe(double.PositiveInfinity);
    }

    [Fact]
    public async Task DivideByZero2()
    {
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(0, BinOp.Divide, 0);
        result.ShouldBe(double.NaN);
    }

    [Fact]
    public async Task Add()
    {
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(4, BinOp.Add, 3);
        result.ShouldBe(7);
    }

    [Fact]
    public async Task Subtract()
    {
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(4, BinOp.Subtract, 3);
        result.ShouldBe(1);
    }
}
