using Shouldly;
using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using XGrpcClient;
using XGrpcServer;
using System.Collections.Generic;

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

public class  GrpcServerStreamTests
{
    private const string host = "localhost";
    [Fact]
    public async Task GetStream()
    {
        await using CalculatorServer server = new CalculatorServer();
        await using CalculatorClient client = new CalculatorClient(host, server.BoundPort);

        // duration should be ~1.0s
        var result = await client.GetRange(0, 10, TimeSpan.FromSeconds(0.1)).ToListAsyncInternal();
        result.ShouldBeEquivalentTo(Enumerable.Range(0, 10).ToList());
    }

    [Fact]
    public async Task GetStreamTimeout()
    {
        await using CalculatorServer server = new CalculatorServer();
        await using CalculatorClient client = new CalculatorClient(host, server.BoundPort);

        client.MaxCallDuration = TimeSpan.FromSeconds(5);

        // returning the entire stream would take ~10s, but we have a max call
        // duration of 5s, so this call should timeout after ~5s
        var ex = await Assert.ThrowsAsync<RpcException>(async () => { await client.GetRange(0, 10, TimeSpan.FromSeconds(1)).ToListAsyncInternal(); });
        ex.Message.ShouldContain("DeadlineExceeded");
    }
}
