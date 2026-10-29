using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.ChatHub;

public class ChatHubServer : IAppConduit
{
    private static readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IChatHub _handler;

    public ChatHubServer(IChatHub handler)
    {
        _handler = handler;
    }

    public async ValueTask<AppInfo> GetAppInfo(CancellationToken cancellation = default) => await _handler.GetAppInfo(cancellation);

    public async ValueTask<AppResponse> UnaryRequest(AppRequest appRequest, CancellationToken cancellation = default)
    {
        QueryBase? request = _serializer.Deserialize<QueryBase>(appRequest.Payload);
        ReplyBase result;
        try
        {
            result = request switch
            {
                null => new ErrorReply { Code = ErrorCode.InvalidData, Message = "Failed to deserialize request" },
                GetMessagesQuery gmr => new GetMessagesReply() { Envelopes = await _handler.GetAll(gmr.Target, cancellation) },
                SendMessageCmd smc => new BoolReply() { Result = await _handler.SendMessage(smc, cancellation) },
                AckMessageCmd amc => new BoolReply() { Result = await _handler.AckMessage(amc.Target, amc.MessageId, cancellation) },
                _ => new ErrorReply { Code = ErrorCode.InvalidData, Message = $"Unknown request type: {request.GetType().Name}" }
            };
        }
        catch (Exception e)
        {
            result = new ErrorReply { Code = ErrorCode.OtherException, Message = e.Message };
        }
        return new AppResponse(_serializer.Serialize<ReplyBase>(result));
    }

    public async IAsyncEnumerable<AppResponse> ServerStream(AppRequest requestBytes, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        yield return new AppResponse(_serializer.Serialize<ReplyBase>(new ErrorReply { Code = ErrorCode.InvalidOp, Message = "Server streaming is not supported" }));
    }

    public async ValueTask<AppResponse> ClientStream(IAsyncEnumerable<AppRequest> requests, CancellationToken cancellation = default)
    {
        return new AppResponse(_serializer.Serialize<ReplyBase>(new ErrorReply { Code = ErrorCode.InvalidOp, Message = "Client streaming is not supported" }));
    }

    public async IAsyncEnumerable<AppResponse> DuplexStream(IAsyncEnumerable<AppRequest> requests, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        yield return new AppResponse(_serializer.Serialize<ReplyBase>(new ErrorReply { Code = ErrorCode.InvalidOp, Message = "Duplex streaming is not supported" }));
    }
}