using DataFac.Conduits.ProtobufNetClient;
using DataFac.Conduits.ProtobufNetServer;
using DataFac.Conduits.Testing;
using Shouldly;
using System.Threading.Tasks;
using Testing.Calculator;
using Xunit;

namespace DataFac.Conduits.UnitTests;

public class GrpcUnaryRequestTests
{
    private const string host = "localhost";

    [Fact]
    public async Task Multiply()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(3, BinOp.Multiply, 4);
        result.ShouldBe(12);
    }

    [Fact]
    public async Task Divide()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(12, BinOp.Divide, 4);
        result.ShouldBe(3);
    }

    [Fact]
    public async Task DivideByZero()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(12, BinOp.Divide, 0);
        result.ShouldBe(double.PositiveInfinity);
    }

    [Fact]
    public async Task DivideByZero2()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(0, BinOp.Divide, 0);
        result.ShouldBe(double.NaN);
    }

    [Fact]
    public async Task Add()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(4, BinOp.Add, 3);
        result.ShouldBe(7);
    }

    [Fact]
    public async Task Subtract()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        var result = await client.DoBinOp(4, BinOp.Subtract, 3);
        result.ShouldBe(1);
    }
}
