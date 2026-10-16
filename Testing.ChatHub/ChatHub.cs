using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.ChatHub;

public class ChatHub : IChatHub
{
    private readonly ConcurrentDictionary<Address, Mailbox> _messages = new ConcurrentDictionary<Address, Mailbox>();

    public async ValueTask DisposeAsync() { }

    public async ValueTask<Envelope[]> GetAll(Address target, CancellationToken cancellation = default)
    {
        if (_messages.TryGetValue(target, out var mailbox))
            return mailbox.Undelivered.Values.ToArray();
        else
            return Array.Empty<Envelope>();
    }

    public async ValueTask<bool> AckMessage(Address target, MessageId messageId, CancellationToken cancellation = default)
    {
        AckMessageCmd request = new AckMessageCmd() { MessageId = messageId, Target = target };
        if (_messages.TryGetValue(request.Target, out var mailbox))
        {
            return mailbox.TryRemove(request.MessageId);
        }
        return false;
    }

    public async ValueTask<bool> SendMessage(SendMessageCmd request, CancellationToken cancellation = default)
    {
        Mailbox mailbox = _messages.GetOrAdd(request.Target, (address) => new Mailbox());
        return mailbox.TryAdd(new Envelope()
        {
            MessageId = request.MessageId,
            Source = request.Source,
            Target = request.Target,
            Content = request.Content
        });
    }
}
