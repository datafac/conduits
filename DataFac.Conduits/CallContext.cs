using System;
using System.Threading;

namespace DataFac.Conduits
{
    /// <summary>
    /// todo remove this type
    /// </summary>
    public readonly struct CallContext
    {
        public readonly CancellationToken Token;
        public readonly DateTime? DeadlineUtc;

        public CallContext()
        {
            Token = CancellationToken.None;
            DeadlineUtc = null;
        }
        public CallContext(CancellationToken token)
        {
            Token = token;
            DeadlineUtc = null;
        }
        public CallContext(DateTime? deadlineUtc)
        {
            Token = CancellationToken.None;
            DeadlineUtc = deadlineUtc;
        }
        public CallContext(CancellationToken token, DateTime? deadlineUtc)
        {
            Token = token;
            DeadlineUtc = deadlineUtc;
        }

        public long? GetDeadlineTicks()
        {
            return DeadlineUtc.HasValue ? DeadlineUtc.Value.Ticks : null;
        }

        public bool IsBeforeDeadline(DateTime currentUtc)
        {
            return !DeadlineUtc.HasValue || currentUtc <= DeadlineUtc.Value;
        }
    }
}