using DataFac.Conduits.ProtobufNetClient;
using DataFac.Conduits.ProtobufNetServer;
using DataFac.Conduits.Testing;
using Grpc.Core;
using Shouldly;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Testing.Calculator;
using Xunit;

namespace DataFac.Conduits.UnitTests;

public class ProtobufNetServerStreamTests
{
    private const string host = "localhost";

    [Fact]
    public async Task GetStream()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ProtocolServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtocolClient(new ProtobufGrpcClient(host, server.BoundPort)));

        // duration should be ~1.0s
        var result = await client.GetRange(0, 10, TimeSpan.FromSeconds(0.1), ct).ToListAsyncInternal();
        result.ShouldBeEquivalentTo(Enumerable.Range(0, 10).ToList());
    }

    [Fact]
    public async Task GetStreamWithDeadline()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ProtocolServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtocolClient(new ProtobufGrpcClient(host, server.BoundPort), TimeSpan.FromSeconds(5), timeProvider));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        var ex = await Assert.ThrowsAsync<RpcException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), ct).ToListAsyncInternal(); });
        ex.Message.ShouldContain("DeadlineExceeded");
    }

    [Fact]
    public async Task GetStreamWithCancellation()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ProtocolServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtocolClient(new ProtobufGrpcClient(host, server.BoundPort)));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var ex = await Assert.ThrowsAsync<RpcException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), cts.Token).ToListAsyncInternal(); });
        ex.Message.ShouldContain("Cancelled");
    }
}
