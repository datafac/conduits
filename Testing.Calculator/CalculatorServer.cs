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
    private static readonly ReadOnlyMemory<byte> errorDeadlineExceeded
        = serializer.Serialize<ResultBase>(
            new ErrorResult
            {
                Code = ExcpCode.DeadlineExceededqqq,
                Message = "Completion deadline exceeded"
            });
    private static readonly ReadOnlyMemory<byte> errorDeserializationFailure
        = serializer.Serialize<ResultBase>(
            new ErrorResult
            {
                Code = ExcpCode.DeserializationError,
                Message = "Failed to deserialize request"
            });

    private readonly IAsyncCalculator _calculator;

    public string ServerName => throw new NotImplementedException();

    public string ServerVersion => throw new NotImplementedException();

    public CalculatorServer(IAsyncCalculator calculator)
    {
        _calculator = calculator;
    }

    public async ValueTask DisposeAsync()
    {
    }

    public async ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ReadOnlyMemory<byte> requestBytes, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        if (deadlineUtc.HasValue && deadlineUtc.Value < DateTime.UtcNow) return errorDeadlineExceeded; // todo use TimeProvider
        RequestBase? request = serializer.Deserialize<RequestBase>(requestBytes);
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
        return serializer.Serialize<ResultBase>(result);
    }

    public async IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> requestBytes, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        if (deadlineUtc.HasValue && deadlineUtc.Value < DateTime.UtcNow) // todo use TimeProvider
        {
            yield return errorDeadlineExceeded;
            yield break;
        }
        RequestBase? request = serializer.Deserialize<RequestBase>(requestBytes);
        if (request is null)
        {
            yield return errorDeserializationFailure;
        }
        else if (request is RangeRequest rr)
        {
            await foreach (int x in _calculator.GetRange(rr.Start, rr.Count, rr.Delay, cancellation).ConfigureAwait(false))
            {
                if (deadlineUtc.HasValue && deadlineUtc.Value < DateTime.UtcNow) // todo use TimeProvider
                {
                    yield return errorDeadlineExceeded;
                    yield break;
                }
                yield return serializer.Serialize<ResultBase>(new RangeResult() { X = x });
            }
        }
        else
        {
            yield return serializer.Serialize<ResultBase>(new ErrorResult { Code = ExcpCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" });
        }
    }

    public ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }
}
