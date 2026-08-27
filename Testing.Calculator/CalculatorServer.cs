using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Calculator;

public class CalculatorServer : IConduitServer
{
    private static readonly MessagePackSerializer serializer = new MessagePackSerializer();
    private static readonly ConduitResponse errorDeadlineExceeded
        = new ConduitResponse(serializer.Serialize<ResultBase>(
            new ErrorResult
            {
                Code = ExcpCode.DeadlineExceededqqq,
                Message = "Completion deadline exceeded"
            }));
    private static readonly ConduitResponse errorDeserializationFailure
        = new ConduitResponse(serializer.Serialize<ResultBase>(
            new ErrorResult
            {
                Code = ExcpCode.DeserializationError,
                Message = "Failed to deserialize request"
            }));

    private readonly TimeProvider _timeProvider;
    public TimeProvider TimeProvider => _timeProvider;

    private readonly IAsyncCalculator _calculator;

    public string ServerName => throw new NotImplementedException();

    public string ServerVersion => throw new NotImplementedException();

    public CalculatorServer(IAsyncCalculator calculator, TimeProvider? timeProvider)
    {
        _calculator = calculator;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async ValueTask DisposeAsync()
    {
    }

    public async ValueTask<ConduitResponse> SimpleUnaryCall(ConduitRequest requestBytes, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (deadlineUtc.HasValue && deadlineUtc.Value < _timeProvider.GetUtcNow().UtcDateTime) return errorDeadlineExceeded;
        RequestBase? request = serializer.Deserialize<RequestBase>(requestBytes.Payload);
        if (request is null) return errorDeserializationFailure;
        ResultBase result;
        try
        {
            result = request switch
            {
                BinOpRequest br => new UnaryResult() { X = await _calculator.DoBinOp(br.A, br.Op, br.B) },
                _ => new ErrorResult { Code = ExcpCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" }
            };
        }
        catch (DivideByZeroException e)
        {
            result = new ErrorResult { Code = ExcpCode.DivideByZero, Message = e.Message };
        }
        catch (OverflowException e)
        {
            result = new ErrorResult { Code = ExcpCode.Overflow, Message = e.Message };
        }
        catch (Exception e)
        {
            result = new ErrorResult { Code = ExcpCode.OtherException, Message = e.Message };
        }
        return new ConduitResponse(serializer.Serialize<ResultBase>(result));
    }

    public async IAsyncEnumerable<ConduitResponse> ServerStream(ConduitRequest requestBytes, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        if (deadlineUtc.HasValue && deadlineUtc.Value < _timeProvider.GetUtcNow().UtcDateTime)
        {
            yield return errorDeadlineExceeded;
            yield break;
        }
        RequestBase? request = serializer.Deserialize<RequestBase>(requestBytes.Payload);
        if (request is null)
        {
            yield return errorDeserializationFailure;
            yield break;
        }
        else if (request is RangeRequest rr)
        {
            await foreach (int x in _calculator.GetRange(rr.Start, rr.Count, rr.Delay, cancellation).ConfigureAwait(false))
            {
                if (deadlineUtc.HasValue && deadlineUtc.Value < _timeProvider.GetUtcNow().UtcDateTime)
                {
                    yield return errorDeadlineExceeded;
                    yield break;
                }
                yield return new ConduitResponse(serializer.Serialize<ResultBase>(new RangeResult() { X = x }));
            }
        }
        else
        {
            yield return new ConduitResponse(serializer.Serialize<ResultBase>(new ErrorResult { Code = ExcpCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" }));
        }
    }

    public ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ConduitRequest> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }
}
