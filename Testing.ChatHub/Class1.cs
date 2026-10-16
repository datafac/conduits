using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.ChatHub;

public interface IChatHub : IAsyncDisposable
{
    //ValueTask<GetMessagesResponse> GetMessages(GetMessagesRequest request, CancellationToken cancellation = default);
    //ValueTask<SendMessageResponse> SendMessage(SendMessageRequest request, CancellationToken cancellation = default);
}

internal sealed class Mailbox
{
    private ImmutableQueue<Envelope> _undelivered = ImmutableQueue<Envelope>.Empty;
    public ImmutableQueue<Envelope> Undelivered => _undelivered;
    public void Enqueue(Envelope envelope) => ImmutableInterlocked.Enqueue(ref _undelivered, envelope);
}

public class ChatHub : IChatHub
{
    private readonly ConcurrentDictionary<Address, Mailbox> _messages = new ConcurrentDictionary<Address, Mailbox>();

    public async ValueTask DisposeAsync() { }

    public ValueTask<Envelopes> GetMessages(GetMessagesRequest request, CancellationToken cancellation = default)
    {
        return _messages.TryRemove(request.Username, out var mailbox)
            ? new ValueTask<Envelopes>(new Envelopes() { Batch = mailbox.Undelivered.ToArray() })
            : new ValueTask<Envelopes>(new Envelopes() { Batch = Array.Empty<Envelope>() });
    }
}

public class WeatherClient : IChatHub
{
    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IAppConduit _userChannel;

    public WeatherClient(IAppConduit userChannel)
    {
        _userChannel = userChannel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_userChannel is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

    private static ResultBase HandleResult(ResultBase? result)
    {
        return result switch
        {
            null => throw new InvalidDataException("Failed to deserialise result"),
            ErrorResult errorResult => errorResult.Code switch
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
}

public class WeatherServer : IAppConduit
{
    private static readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IChatHub _handler;

    public WeatherServer(IChatHub handler)
    {
        _handler = handler;
    }

    public async ValueTask<AppInfo> GetAppInfo()
    {
        return new AppInfo($"{ThisAssembly.AssemblyName}.{nameof(WeatherServer)}", new Version(ThisAssembly.AssemblyFileVersion));
    }

    public async ValueTask<AppResponse> UnaryRequest(AppRequest appRequest, CancellationToken cancellation = default)
    {
        var result = new AppResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.InvalidOp, Message = "Not implemented yet" }));
        return result;
    }

    public async IAsyncEnumerable<AppResponse> ServerStream(AppRequest requestBytes, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var result = new AppResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.InvalidOp, Message = "Server streaming is not supported" }));
        yield return result;
    }

    public async ValueTask<AppResponse> ClientStream(IAsyncEnumerable<AppRequest> requests, CancellationToken cancellation = default)
    {
        var result = new AppResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.InvalidOp, Message = "Client streaming is not supported" }));
        return result;
    }

    public async IAsyncEnumerable<AppResponse> DuplexStream(IAsyncEnumerable<AppRequest> requests, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var result = new AppResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.InvalidOp, Message = "Duplex streaming is not supported" }));
        yield return result;
    }

}