using System.Collections.Immutable;

namespace Testing.ChatHub;

internal sealed class Mailbox
{
    private ImmutableDictionary<MessageId, Envelope> _undelivered = ImmutableDictionary<MessageId, Envelope>.Empty;
    public ImmutableDictionary<MessageId, Envelope> Undelivered => _undelivered;
    public bool TryAdd(Envelope envelope) => ImmutableInterlocked.TryAdd(ref _undelivered, envelope.MessageId, envelope);
    public bool TryRemove(MessageId messageId) => ImmutableInterlocked.TryRemove(ref _undelivered, messageId, out var _);
}
