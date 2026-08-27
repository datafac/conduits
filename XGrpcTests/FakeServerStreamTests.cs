using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using DataFac.Conduits.Testing;
using Testing.Calculator;
using System.Threading;

namespace XGrpcTests;

public class FakeServerStreamTests
{
    [Fact]
    public async Task GetStream()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new CalculatorServer(new Calculator(), timeProvider), timeProvider);
        await using var client = new CalculatorClient(new FakeConduitClient(server, timeProvider));

        // duration should be ~1.0s
        var result = await client.GetRange(0, 10, TimeSpan.FromSeconds(0.1)).ToListAsyncInternal();
        result.ShouldBeEquivalentTo(Enumerable.Range(0, 10).ToList());
    }

    [Fact]
    public async Task GetStreamWithDeadline()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new CalculatorServer(new Calculator(), timeProvider), timeProvider);
        await using var client = new CalculatorClient(new FakeConduitClient(server, timeProvider));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        client.MaxCallDuration = TimeSpan.FromSeconds(5);
        var ex = await Assert.ThrowsAsync<TimeoutException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1)).ToListAsyncInternal(); });
        ex.Message.ShouldBe("Completion deadline exceeded");
    }

    [Fact]
    public async Task GetStreamWithCancellation()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new CalculatorServer(new Calculator(), timeProvider), timeProvider);
        await using var client = new CalculatorClient(new FakeConduitClient(server, timeProvider));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        client.MaxCallDuration = null;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), cts.Token).ToListAsyncInternal(); });
        ex.Message.ShouldContain("Cancelled by caller");
    }
}
