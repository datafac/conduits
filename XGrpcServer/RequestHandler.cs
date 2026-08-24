using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Testing.Calculator;
using XGrpcShared;

namespace XGrpcServer;

internal class RequestHandler : IRequestHandler
{
    private static readonly MessagePackSerializer serializer = new MessagePackSerializer();
    private static readonly ReadOnlyMemory<byte> errorDeserializationFailure
        = serializer.Serialize<ResultBase>(
            new ErrorResult
            {
                Code = ExcpCode.UnknownOther,
                Message = "Failed to deserialize request"
            });

    private readonly IAsyncCalculator _calculator;

    public RequestHandler(IAsyncCalculator calculator)
    {
        _calculator = calculator;
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleUnaryRequest(ReadOnlyMemory<byte> requestBytes, CancellationToken cancellation)
    {
        RequestBase? request = serializer.Deserialize<RequestBase>(requestBytes);
        if (request is null) return errorDeserializationFailure;
        ResultBase result;
        try
        {
            result = request switch
            {
                BinOpRequest br => new UnaryResult() { X = await _calculator.DoBinOp(br.A, br.Op, br.B) },
                _ => new ErrorResult { Code = ExcpCode.UnknownRequest, Message = $"Unknown request type: {request.GetType().Name}" }
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
            result = new ErrorResult { Code = ExcpCode.UnknownOther, Message = e.Message };
        }
        return serializer.Serialize<ResultBase>(result);
    }

    public async IAsyncEnumerable<ResultBase> HandleServerStream(RequestBase request, [EnumeratorCancellation] CancellationToken cancellation)
    {
        if (request is RangeRequest rr)
        {
            await foreach (int x in _calculator.GetRange(rr.Start, rr.Count, rr.Delay).WithCancellation(cancellation).ConfigureAwait(false))
            {
                yield return new RangeResult() { X = x };
            }
        }
        else
        {
            yield return new ErrorResult { Code = ExcpCode.UnknownRequest, Message = $"Unknown request type: {request.GetType().Name}" };
        }
    }
}
