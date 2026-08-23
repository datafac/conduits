using System;
using System.Threading;

namespace DataFac.Conduits
{
    public sealed class ComboTokenSource : IDisposable
    {
        private readonly CancellationTokenSource? _timerTokenSource = null;
        private readonly CancellationTokenSource? _comboTokenSource = null;
        private readonly CancellationToken _token;

        public ComboTokenSource(CancellationToken token, DateTime? deadlineUtc, DateTime currentUtc)
        {
            _token = token;

            if (deadlineUtc.HasValue)
            {
                _timerTokenSource = new CancellationTokenSource(deadlineUtc.Value - currentUtc);
                _comboTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_token, _timerTokenSource.Token);
            }
        }

        public ComboTokenSource(CallContext context, DateTime currentUtc) : this(context.Token, context.DeadlineUtc, currentUtc) { }

        public CancellationToken Token => _comboTokenSource is not null ? _comboTokenSource.Token : _token;

        public void Dispose()
        {
            _timerTokenSource?.Dispose();
            _comboTokenSource?.Dispose();
        }
    }
}