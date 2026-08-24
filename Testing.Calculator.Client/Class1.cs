using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Testing.Calculator.Client
{
    public class CalculatorClient : IAsyncCalculator
    {
        private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

        private readonly IConduitClient _conduitClient;

        public CalculatorClient(IConduitClient conduitClient)
        {
            _conduitClient = conduitClient;
        }

        public async ValueTask DisposeAsync()
        {
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

        private async ValueTask<ResultBase?> UnaryCall(RequestBase request)
        {
            var deadline = DateTime.UtcNow + MaxCallDuration; // todo use time provider
            var requestBytes = _serializer.Serialize<RequestBase>(request);
            var resultBytes = await _conduitClient.SimpleUnaryCall(requestBytes, deadline).ConfigureAwait(false);
            return _serializer.Deserialize<ResultBase>(resultBytes);
        }

        public async ValueTask<double> DoBinOp(double a, BinOp op, double b)
        {
            RequestBase req = new BinOpRequest() { A = a, Op = op, B = b };
            ResultBase result = HandleResult(await UnaryCall(req).ConfigureAwait(false));
            if (result is UnaryResult ur)
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
            var deadline = DateTime.UtcNow + MaxCallDuration; // todo use time provider
            var requestBytes = _serializer.Serialize<RequestBase>(request);
            await foreach (var resultBytes in _conduitClient.ServerStream(requestBytes, deadline).ConfigureAwait(false))
            {
                var result = _serializer.Deserialize<ResultBase>(resultBytes);
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
}
