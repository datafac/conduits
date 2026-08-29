using Shouldly;
using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DataFac.Conduits.ProtobufNetClient;
using DataFac.Conduits.ProtobufNetServer;
using DataFac.Conduits.Testing;
using Testing.Calculator;
using System.Threading;
using DataFac.Conduits;

namespace XGrpcTests;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

/// <summary>
/// Helpers provided for older versions of .NET because Microsoft hasn't.
/// </summary>
internal static class AsyncEnumerableHelpers
{
    public static async ValueTask<List<T>> ToListAsyncInternal<T>(this IAsyncEnumerable<T> source)
    {
        var result = new List<T>();
        await foreach (var item in source)
        {
            result.Add(item);
        }
        return result;
    }
}

public class ProtobufNetServerStreamTests
{
    private const string host = "localhost";

    [Fact]
    public async Task GetStream()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        // duration should be ~1.0s
        client.MaxCallDuration = null;
        var result = await client.GetRange(0, 10, TimeSpan.FromSeconds(0.1), ct).ToListAsyncInternal();
        result.ShouldBeEquivalentTo(Enumerable.Range(0, 10).ToList());
    }

    [Fact]
    public async Task GetStreamWithDeadline()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        client.MaxCallDuration = TimeSpan.FromSeconds(5);
        var ex = await Assert.ThrowsAsync<RpcException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), ct).ToListAsyncInternal(); });
        ex.Message.ShouldContain("DeadlineExceeded");
    }

    [Fact]
    public async Task GetStreamWithCancellation()
    {
        var timeProvider = new FakeTimeProvider();
        await using var server = new ProtobufGrpcServer(new ConduitServer(timeProvider, new CalculatorServer(new Calculator())));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        client.MaxCallDuration = null;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var ex = await Assert.ThrowsAsync<RpcException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), cts.Token).ToListAsyncInternal(); });
        ex.Message.ShouldContain("Cancelled");
    }
}
