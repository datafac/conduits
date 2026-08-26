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
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        // duration should be ~1.0s
        client.MaxCallDuration = null;
        var result = await client.GetRange(0, 10, TimeSpan.FromSeconds(0.1)).ToListAsyncInternal();
        result.ShouldBeEquivalentTo(Enumerable.Range(0, 10).ToList());
    }

    [Fact]
    public async Task GetStreamWithDeadline()
    {
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        client.MaxCallDuration = TimeSpan.FromSeconds(5);
        var ex = await Assert.ThrowsAsync<RpcException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1)).ToListAsyncInternal(); });
        ex.Message.ShouldContain("DeadlineExceeded");
    }

    [Fact]
    public async Task GetStreamWithCancellation()
    {
        await using var server = new ProtobufGrpcServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new ProtobufGrpcClient(host, server.BoundPort));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        client.MaxCallDuration = null;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var ex = await Assert.ThrowsAsync<RpcException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), cts.Token).ToListAsyncInternal(); });
        ex.Message.ShouldContain("Cancelled");
    }
}
public class FakeServerStreamTests
{
    [Fact]
    public async Task GetStream()
    {
        await using var server = new FakeConduitServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new FakeConduitClient(server));

        // duration should be ~1.0s
        var result = await client.GetRange(0, 10, TimeSpan.FromSeconds(0.1)).ToListAsyncInternal();
        result.ShouldBeEquivalentTo(Enumerable.Range(0, 10).ToList());
    }

    [Fact]
    public async Task GetStreamWithDeadline()
    {
        await using var server = new FakeConduitServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new FakeConduitClient(server));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        client.MaxCallDuration = TimeSpan.FromSeconds(5);
        var ex = await Assert.ThrowsAsync<TimeoutException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1)).ToListAsyncInternal(); });
        ex.Message.ShouldBe("Completion deadline exceeded");
    }

    [Fact]
    public async Task GetStreamWithCancellation()
    {
        await using var server = new FakeConduitServer(new CalculatorServer(new Calculator()));
        await using var client = new CalculatorClient(new FakeConduitClient(server));

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        client.MaxCallDuration = null;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1), cts.Token).ToListAsyncInternal(); });
        ex.Message.ShouldContain("Cancelled by caller");
    }
}
