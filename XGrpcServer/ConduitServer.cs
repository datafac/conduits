using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nerdbank.MessagePack;
using ProtoBuf.Grpc;
using Testing.Calculator;
using XGrpcShared;

namespace XGrpcServer;

internal class ConduitServer : IConduit
{
    private static readonly MessagePackSerializer serializer = new MessagePackSerializer();
    private readonly IRequestHandler _requestHandler;

    public ConduitServer(IRequestHandler requestHandler)
    {
        _requestHandler = requestHandler;
    }

    private static readonly ResultBlob errorDeserializationFailureResult
        = new()
        {
            Blob = serializer.Serialize<ResultBase>(
            new ErrorResult
            {
                Code = ExcpCode.UnknownOther,
                Message = "Failed to deserialize request"
            })
        };

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
        var result = await _requestHandler.HandleUnaryRequest(requestBlob.Blob, requestCts.Token);
        return new ResultBlob() { Blob = result.ToArray() }; // todo remove ToArray()
    }

    public async IAsyncEnumerable<ResultBlob> ServerStream(RequestBlob requestBlob, CallContext context)
    {
        TimeSpan timeout = GetMaxCallDuration(context);
        using var deadlineCts = new CancellationTokenSource(timeout);
        using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, deadlineCts.Token);
        RequestBase? request = serializer.Deserialize<RequestBase>(requestBlob.Blob);
        if (request is null)
        {
            yield return errorDeserializationFailureResult;
        }
        else
        {
            await foreach (var result in _requestHandler.HandleServerStream(request, requestCts.Token))
            {
                yield return new ResultBlob() { Blob = serializer.Serialize<ResultBase>(result) };
            }
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