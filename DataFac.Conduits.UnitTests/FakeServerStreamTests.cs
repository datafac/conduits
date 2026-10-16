using DataFac.Conduits.Testing;
using Shouldly;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Testing.Calculator;
using Xunit;

namespace DataFac.Conduits.UnitTests;

public class ConduitInfoTests
{
    [Fact]
    public void ParseFrom_ValidInput()
    {
        var input = "Name=TestConduit;Version=1.2.3.4";
        var info = ConduitInfo.ParseFrom(input);
        info.Name.ShouldBe("TestConduit");
        info.Version.ShouldBe(new Version(1, 2, 3, 4));
    }

    [Theory]
    [InlineData("  ")]
    [InlineData("Name;Version=1.2.3.4")]
    [InlineData("Name=UnitTest;Version")]
    [InlineData("Name=UnitTest;Version=invalid")]
    public void ParseFrom_InvalidInput_ThrowsArgumentException(string invalidInput)
    {
        Assert.Throws<ArgumentException>(() => ConduitInfo.ParseFrom(invalidInput));
    }
}

public class FakeServerStreamTests
{
    [Fact]
    public async Task GetConduitInfo()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator()), true), true);
        await using var client = new FakeConduitClient(server, timeProvider);

        // duration should be ~1.0s
        var result = await client.GetConduitInfoAsync();
        result.Name.ShouldBe("DataFac.Conduits.Testing.FakeConduitClient");
        result.Version.ToString().ShouldStartWith("1.0.");
    }

    [Fact]
    public async Task GetStream()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator()), true), true);
        await using var client = new CalculatorClient(new ConduitClient(new FakeConduitClient(server, timeProvider), true));

        // duration should be ~1.0s
        var result = await client.GetRange(0, 10, TimeSpan.FromSeconds(0.1), ct).ToListAsyncInternal();
        result.ShouldBeEquivalentTo(Enumerable.Range(0, 10).ToList());
    }

    [Fact]
    public async Task GetStreamWithDeadline()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator()), true), true);
        await using var client = new CalculatorClient(new ConduitClient(new FakeConduitClient(server, timeProvider), false, TimeSpan.FromSeconds(5), timeProvider));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        var ex = await Assert.ThrowsAsync<TimeoutException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), ct).ToListAsyncInternal(); });
        ex.Message.ShouldBe("Deadline exceeded");
    }

    [Fact]
    public async Task GetStreamWithCancellation()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new FakeConduitServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator()), true), true);
        await using var client = new CalculatorClient(new ConduitClient(new FakeConduitClient(server, timeProvider), true));

        // returning the entire stream would take ~10s, but we have a cancellation
        // after 5s, so this call should timeout after ~5s
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), cts.Token).ToListAsyncInternal(); });
        ex.Message.ShouldContain("Operation cancelled");
    }
}
