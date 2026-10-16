using Nerdbank.MessagePack;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.ChatHub;

/// <summary>
/// Represents a chat service running over request/response comms.
/// </summary>
public interface IChatHub : IAsyncDisposable
{
    /// <summary>
    /// Request/response to get all undelivered messages.
    /// Todo replace this with subscriptions.
    /// </summary>
    /// <returns></returns>
    ValueTask<Envelope[]> GetAll(Address target, CancellationToken cancellation = default);

    /// <summary>
    /// Command to achknowledge message receipt. Causes deletion in the mailbox.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    ValueTask<bool> AckMessage(Address target, MessageId messageId, CancellationToken cancellation = default);

    /// <summary>
    /// Sends a message to a target mailbox.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    ValueTask<bool> SendMessage(SendMessageCmd request, CancellationToken cancellation = default);
}
