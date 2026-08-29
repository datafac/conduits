using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataFac.Conduits.ProtobufNetCommon;

namespace DataFac.Conduits.ProtobufNetServer;

internal static class PayloadExtensions
{
    public static ResultBlob ToResultBlob(this ConduitResponse response)
    {
        return new ResultBlob()
        {
            Control = (int)response.Control,
            Payload = response.Payload.ToArray() // todo alloc!
        };
    }

    public static ConduitRequest ToConduitRequest(this RequestBlob request)
    {
        return new ConduitRequest(
            (ControlCode)request.Control,
            new ReadOnlyMemory<byte>(request.Payload));
    }
}

internal class ProtobufNetServer : IProtobufNetContract
{
    private readonly IConduitServer _requestHandler;

    public ProtobufNetServer(IConduitServer requestHandler)
    {
        _requestHandler = requestHandler;
    }

    private static TimeSpan GetMaxCallDuration(CallContext context)
    {
        DateTime deadline = context.Deadline ?? DateTime.UtcNow.AddHours(1);
        TimeSpan duration = deadline - DateTime.UtcNow;
        if (duration > TimeSpan.FromHours(1))
        {
            duration = TimeSpan.FromHours(1);
        }
        return duration;
    }

    public async ValueTask<ResultBlob> UnaryRequest(RequestBlob requestBlob, CallContext context)
    {
        TimeSpan timeout = GetMaxCallDuration(context);
        using var deadlineCts = new CancellationTokenSource(timeout);
        using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, deadlineCts.Token);
        var result = await _requestHandler.UnaryRequest(requestBlob.ToConduitRequest(), context.Deadline, requestCts.Token);
        return result.ToResultBlob(); 
    }

    public async IAsyncEnumerable<ResultBlob> ServerStream(RequestBlob requestBlob, CallContext context)
    {
        TimeSpan timeout = GetMaxCallDuration(context);
        using var deadlineCts = new CancellationTokenSource(timeout);
        using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, deadlineCts.Token);
        await foreach (var result in _requestHandler.ServerStream(requestBlob.ToConduitRequest(), context.Deadline, requestCts.Token))
        {
            yield return result.ToResultBlob();
        }
    }

    public ValueTask<ResultBlob> ClientStream(IAsyncEnumerable<RequestBlob> requests, CallContext context = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ResultBlob> DuplexStream(IAsyncEnumerable<RequestBlob> requests, CallContext context = default)
    {
        throw new NotImplementedException();
    }
}
