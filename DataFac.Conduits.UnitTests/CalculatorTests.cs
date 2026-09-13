using DataFac.Conduits.ProtobufNetClient;
using DataFac.Conduits.ProtobufNetServer;
using DataFac.Conduits.Testing;
using Shouldly;
using System.Threading.Tasks;
using Testing.Calculator;
using Xunit;

namespace DataFac.Conduits.UnitTests;

public class CalculatorTests
{
    private const string host = "localhost";

    [Fact]
    public async Task AppInfo()
    {
        await using var client = new CalculatorClient(new CalculatorServer(new Calculator()));

        string appInfo = await client.GetAppInfo();
        appInfo.ShouldStartWith("AppName=Testing.Calculator;Version=1.0.");
    }

    [Theory]
    [InlineData(3, BinOp.Multiply, 4, 12)]
    [InlineData(12, BinOp.Divide, 4, 3)]
    [InlineData(12, BinOp.Divide, 0, double.PositiveInfinity)]
    [InlineData(0, BinOp.Divide, 0, double.NaN)]
    [InlineData(4, BinOp.Add, 3, 7)]
    [InlineData(4, BinOp.Subtract, 3, 1)]
    public async Task BinOp_Direct(double a, BinOp op, double b, double expected)
    {
        await using var client = new CalculatorClient(new CalculatorServer(new Calculator()));

        var ct = TestContext.Current.CancellationToken;

        var result = await client.DoBinOp(a, op, b, ct);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(3, BinOp.Multiply, 4, 12)]
    [InlineData(12, BinOp.Divide, 4, 3)]
    [InlineData(12, BinOp.Divide, 0, double.PositiveInfinity)]
    [InlineData(0, BinOp.Divide, 0, double.NaN)]
    [InlineData(4, BinOp.Add, 3, 7)]
    [InlineData(4, BinOp.Subtract, 3, 1)]
    public async Task BinOp_OverProtocol(double a, BinOp op, double b, double expected)
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ConduitServer(timeProvider, new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ConduitClient(server));

        var ct = TestContext.Current.CancellationToken;

        var result = await client.DoBinOp(a, op, b, ct);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(3, BinOp.Multiply, 4, 12)]
    [InlineData(12, BinOp.Divide, 4, 3)]
    [InlineData(12, BinOp.Divide, 0, double.PositiveInfinity)]
    [InlineData(0, BinOp.Divide, 0, double.NaN)]
    [InlineData(4, BinOp.Add, 3, 7)]
    [InlineData(4, BinOp.Subtract, 3, 1)]
    public async Task BinOp_ViaProtobufNet(double a, BinOp op, double b, double expected)
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ConduitClient(new ProtobufGrpcClient(host, server.BoundPort)));

        var ct = TestContext.Current.CancellationToken;

        var result = await client.DoBinOp(a, op, b, ct);
        result.ShouldBe(expected);
    }
}
