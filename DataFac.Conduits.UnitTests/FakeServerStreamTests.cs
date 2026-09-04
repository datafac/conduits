using DataFac.Conduits.Testing;
using Shouldly;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Testing.Calculator;
using Xunit;

namespace DataFac.Conduits.UnitTests;

public class FakeServerStreamTests
{
    [Fact]
    public async Task GetStream()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new ProtocolServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtocolClient(new FakeConduitClient(server, timeProvider)));

        // duration should be ~1.0s
        var result = await client.GetRange(0, 10, TimeSpan.FromSeconds(0.1), ct).ToListAsyncInternal();
        result.ShouldBeEquivalentTo(Enumerable.Range(0, 10).ToList());
    }

    [Fact]
    public async Task GetStreamWithDeadline()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new ProtocolServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtocolClient(new FakeConduitClient(server, timeProvider), TimeSpan.FromSeconds(5), timeProvider));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        var ex = await Assert.ThrowsAsync<TimeoutException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), ct).ToListAsyncInternal(); });
        ex.Message.ShouldBe("Deadline exceeded");
    }

    [Fact]
    public async Task GetStreamWithCancellation()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new ProtocolServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtocolClient(new FakeConduitClient(server, timeProvider)));

        // returning the entire stream would take ~10s, but we have a cancellation
        // after 5s, so this call should timeout after ~5s
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), cts.Token).ToListAsyncInternal(); });
        ex.Message.ShouldContain("Operation cancelled");
    }
}
