using Grpc.Core;
using Nerdbank.MessagePack;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testing.Calculator;
using XGrpcShared;

namespace XGrpcClient;

public class CalculatorClient : IAsyncCalculator
{
    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly Channel channel;
    private readonly IConduit conduit;

    public CalculatorClient(string server, int port)
    {
        channel = new Channel(server, port, ChannelCredentials.Insecure);
        conduit = channel.CreateGrpcService<IConduit>();
    }

    public async ValueTask DisposeAsync()
    {
        await channel.ShutdownAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    public TimeSpan MaxCallDuration
    {
        get;
        set
        {
            field = value < TimeSpan.FromSeconds(5)
                ? TimeSpan.FromSeconds(5)
                : value > TimeSpan.FromSeconds(300)
                    ? TimeSpan.FromSeconds(300)
                    : value;
        }
    } = TimeSpan.FromSeconds(30);

    private async ValueTask<ResultBase?> UnaryCall(RequestBase request)
    {
        var requestBlob = new RequestBlob() { Blob = _serializer.Serialize<RequestBase>(request) };
        CallOptions callOptions = new CallOptions(deadline: DateTime.UtcNow + MaxCallDuration);
        var resultBlob = await conduit.UnaryRequest(requestBlob, new CallContext(callOptions)).ConfigureAwait(false);
        return _serializer.Deserialize<ResultBase>(resultBlob.Blob);
    }

    private static ResultBase HandleResult(ResultBase? result)
    {
        return result switch
        {
            null => throw new Exception("Failed to deserialise result"),
            ErrorResult errorResult => errorResult.Code switch
            {
                ExcpCode.DivideByZero => throw new DivideByZeroException(errorResult.Message),
                ExcpCode.Overflow => throw new OverflowException(errorResult.Message),
                ExcpCode.UnknownOther => throw new Exception(errorResult.Message),
                ExcpCode.UnknownRequest => throw new Exception(errorResult.Message),
                _ => throw new Exception($"Unknown error code: {errorResult.Code}")
            },
            _ => result
        };
    }

    public async ValueTask<double> DoBinOp(double x, BinOp op, double y)
    {
        RequestBase req = new BinOpRequest() { A = x, Op = op, B = y };
        ResultBase result = HandleResult(await UnaryCall(req).ConfigureAwait(false));
        if(result is UnaryResult ur)
        {
            return ur.X;
        }
        else
        {
            throw new Exception($"Unexpected result type: {result.GetType().Name}");
        }
    }

    private async IAsyncEnumerable<ResultBase> ServerStream(RequestBase request)
    {
        var requestBlob = new RequestBlob() { Blob = _serializer.Serialize<RequestBase>(request) };
        CallOptions callOptions = new CallOptions(deadline: DateTime.UtcNow + MaxCallDuration);
        await foreach(ResultBlob resultBlob in conduit.ServerStream(requestBlob, new CallContext(callOptions)).ConfigureAwait(false))
        {
            var result = _serializer.Deserialize<ResultBase>(resultBlob.Blob);
            yield return HandleResult(result);
        }
    }

    public async IAsyncEnumerable<int> GetRange(int start, int count, TimeSpan delay)
    {
        RequestBase req = new RangeRequest() { Start = start, Count = count, Delay = delay };
        await foreach (var result in ServerStream(req).ConfigureAwait(false))
        {
            yield return result switch
            {
                RangeResult rr => rr.X,
                _ => throw new Exception($"Unexpected result type: {result.GetType().Name}")
            };
        }
    }
}
