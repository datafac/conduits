using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Calculator;

public class CalculatorClient : IAsyncCalculator
{
    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IUserChannel _userChannel;

    public CalculatorClient(IUserChannel userChannel)
    {
        _userChannel = userChannel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_userChannel is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

    private static ResultBase HandleResult(ResultBase? result)
    {
        return result switch
        {
            null => throw new InvalidDataException("Failed to deserialise result"),
            ErrorResult errorResult => errorResult.Code switch
            {
                ExcpCode.DeserializationError => throw new InvalidDataException(errorResult.Message),
                ExcpCode.DeadlineExceededqqq => throw new TimeoutException(errorResult.Message),
                ExcpCode.OperationCancelled => throw new OperationCanceledException(errorResult.Message),
                ExcpCode.UnsupportedRequestType => throw new NotSupportedException(errorResult.Message),
                ExcpCode.UnsupportedResponseType => throw new NotSupportedException(errorResult.Message),
                ExcpCode.OtherException => throw new Exception(errorResult.Message),
                ExcpCode.DivideByZero => throw new DivideByZeroException(errorResult.Message),
                ExcpCode.Overflow => throw new OverflowException(errorResult.Message),
                _ => throw new Exception($"Unknown error code: {errorResult.Code}")
            },
            _ => result
        };
    }

    private async ValueTask<ResultBase?> UnaryCall(RequestBase request, CancellationToken cancellation)
    {
        var requestBytes = _serializer.Serialize<RequestBase>(request);
        var response = await _userChannel.UnaryRequest(new UserRequest(requestBytes), cancellation).ConfigureAwait(false);
        return _serializer.Deserialize<ResultBase>(response.Payload);
    }

    public async ValueTask<double> DoBinOp(double a, BinOp op, double b, CancellationToken cancellation = default)
    {
        RequestBase req = new BinOpRequest() { A = a, Op = op, B = b };
        ResultBase result = HandleResult(await UnaryCall(req, cancellation).ConfigureAwait(false));
        if (result is UnaryResult ur)
        {
            return ur.X;
        }
        else
        {
            throw new Exception($"Unexpected result type: {result.GetType().Name}");
        }
    }

    private async IAsyncEnumerable<ResultBase> ServerStream(RequestBase request, [EnumeratorCancellation] CancellationToken cancellation)
    {
        //DateTime? deadline = calculateDeadline();
        ReadOnlyMemory<byte> requestBytes = _serializer.Serialize<RequestBase>(request);
        await foreach (var response in _userChannel.ServerStream(new UserRequest(requestBytes), cancellation).ConfigureAwait(false))
        {
            yield return HandleResult(_serializer.Deserialize<ResultBase>(response.Payload));
        }
    }

    public async IAsyncEnumerable<int> GetRange(int start, int count, TimeSpan delay, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        RequestBase req = new RangeRequest() { Start = start, Count = count, Delay = delay };
        await foreach (var result in ServerStream(req, cancellation).ConfigureAwait(false))
        {
            yield return result switch
            {
                RangeResult rr => rr.X,
                _ => throw new Exception($"Unexpected result type: {result.GetType().Name}")
            };
        }
    }
}
