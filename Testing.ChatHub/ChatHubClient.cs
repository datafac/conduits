using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.ChatHub;

public class ChatHubClient : IChatHub
{
    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IAppConduit _appConduit;

    public ChatHubClient(IAppConduit appConduit)
    {
        _appConduit = appConduit;
    }

    public async ValueTask DisposeAsync()
    {
        if (_appConduit is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

    public async ValueTask<AppInfo> GetAppInfo(CancellationToken cancellation = default) => await _appConduit.GetAppInfo(cancellation);

    private static ReplyBase HandleResult(ReplyBase? result)
    {
        return result switch
        {
            null => throw new InvalidDataException("Failed to deserialise result"),
            ErrorReply errorResult => errorResult.Code switch
            {
                ErrorCode.None => result,
                ErrorCode.InvalidData => throw new InvalidDataException(errorResult.Message),
                ErrorCode.InvalidOp => throw new InvalidOperationException(errorResult.Message),
                ErrorCode.OtherException => throw new Exception(errorResult.Message),
                _ => throw new Exception($"Unknown error code: {errorResult.Code}")
            },
            _ => result
        };
    }

    private async ValueTask<TReply> UnaryCall<TQuery,TReply>(TQuery request, CancellationToken cancellation)
        where TQuery : QueryBase
        where TReply : ReplyBase
    {
        var requestBytes = _serializer.Serialize<QueryBase>(request);
        var response = await _appConduit.UnaryRequest(new AppRequest(requestBytes), cancellation).ConfigureAwait(false);
        var replyBase = _serializer.Deserialize<ReplyBase>(response.Payload);
        ReplyBase reply = HandleResult(replyBase);
        if (reply is TReply expected) return expected;
        throw new Exception($"Unexpected result type: {reply.GetType().Name}");
    }

    public async ValueTask<Envelope[]> GetAll(Address target, CancellationToken cancellation = default)
    {
        var request = new GetMessagesQuery() { Target = target };
        var reply = await UnaryCall<GetMessagesQuery, GetMessagesReply>(request, cancellation);
        return reply.Envelopes;
    }

    public async ValueTask<bool> AckMessage(Address target, MessageId messageId, CancellationToken cancellation = default)
    {
        var request = new AckMessageCmd() { Target = target, MessageId = messageId };
        var reply = await UnaryCall<AckMessageCmd, BoolReply>(request, cancellation);
        return reply.Result;
    }

    public async ValueTask<bool> SendMessage(SendMessageCmd request, CancellationToken cancellation = default)
    {
        var reply = await UnaryCall<SendMessageCmd, BoolReply>(request, cancellation);
        return reply.Result;
    }
}
