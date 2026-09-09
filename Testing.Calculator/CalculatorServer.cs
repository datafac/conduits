using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Calculator;

public class CalculatorServer : IAppConduit
{
    private static readonly MessagePackSerializer serializer = new MessagePackSerializer();
    private static readonly AppResponse errorDeserializationFailure
        = new AppResponse(serializer.Serialize<ResultBase>(
            new ErrorResult
            {
                Code = ErrorCode.DeserializationError,
                Message = "Failed to deserialize request"
            }));

    private readonly IAsyncCalculator _calculator;

    public string ServerName => throw new NotImplementedException();

    public string ServerVersion => throw new NotImplementedException();

    public CalculatorServer(IAsyncCalculator calculator)
    {
        _calculator = calculator;
    }

    public async ValueTask<AppResponse> UnaryRequest(AppRequest requestBytes, CancellationToken cancellation = default)
    {
        RequestBase? request = serializer.Deserialize<RequestBase>(requestBytes.Payload);
        if (request is null) return errorDeserializationFailure;
        ResultBase result;
        try
        {
            result = request switch
            {
                BinOpRequest br => new UnaryResult() { X = await _calculator.DoBinOp(br.A, br.Op, br.B) },
                _ => new ErrorResult { Code = ErrorCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" }
            };
        }
        catch (DivideByZeroException e)
        {
            result = new ErrorResult { Code = ErrorCode.DivideByZero, Message = e.Message };
        }
        catch (OverflowException e)
        {
            result = new ErrorResult { Code = ErrorCode.Overflow, Message = e.Message };
        }
        catch (Exception e)
        {
            result = new ErrorResult { Code = ErrorCode.OtherException, Message = e.Message };
        }
        return new AppResponse(serializer.Serialize<ResultBase>(result));
    }

    public async IAsyncEnumerable<AppResponse> ServerStream(AppRequest requestBytes, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
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
                yield return new AppResponse(serializer.Serialize<ResultBase>(new RangeResult() { X = x }));
            }
        }
        else
        {
            yield return new AppResponse(serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" }));
        }
    }

    public ValueTask<AppResponse> ClientStream(IAsyncEnumerable<AppRequest> requests, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<AppResponse> DuplexStream(IAsyncEnumerable<AppRequest> requests, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }
}
