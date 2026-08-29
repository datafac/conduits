using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Calculator;

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
        if (_conduitClient is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

    private TimeSpan? _maxCallDuration;
    /// <summary>
    /// A duration between 0 and 5 minutes, or null. If not null, this is used to calculate 
    /// a deadline for each call.
    /// </summary>
    public TimeSpan? MaxCallDuration
    {
        get { return _maxCallDuration; }
        set
        {
            if (value is null) _maxCallDuration = null;
            else if (value < TimeSpan.Zero) _maxCallDuration = TimeSpan.Zero;
            else if (value > TimeSpan.FromSeconds(300)) _maxCallDuration = TimeSpan.FromSeconds(300);
            else _maxCallDuration = value;
        }
    }

    private DateTime? calculateDeadline()
    {
        return _maxCallDuration.HasValue
            ? _conduitClient.TimeProvider.GetUtcNow().UtcDateTime + _maxCallDuration.Value
            : null;
    }

    private static string DecodeErrorMessage(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 0) return string.Empty;
        return Encoding.UTF8.GetString(payload.ToArray());
    }

    private static ConduitResponse HandleResponse(ConduitResponse response)
    {
        // todo move this to protocol client
        return response.Control switch
        {
            ControlCode.None => response,
            ControlCode.Timeout => throw new TimeoutException(DecodeErrorMessage(response.Payload.Span)),
            _ => throw new Exception($"Unknown control code: {response.Control}")
        };
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

    private async ValueTask<ResultBase?> UnaryCall(RequestBase request)
    {
        DateTime? deadline = calculateDeadline();
        var requestBytes = _serializer.Serialize<RequestBase>(request);
        var response = await _conduitClient.UnaryRequest(new ConduitRequest(requestBytes), deadline).ConfigureAwait(false);
        var result = HandleResponse(response);
        return _serializer.Deserialize<ResultBase>(result.Payload);
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

    private async IAsyncEnumerable<ResultBase> ServerStream(RequestBase request, CancellationToken cancellation)
    {
        DateTime? deadline = calculateDeadline();
        ReadOnlyMemory<byte> requestBytes = _serializer.Serialize<RequestBase>(request);
        await foreach (var response in _conduitClient.ServerStream(new ConduitRequest(requestBytes), deadline, cancellation).ConfigureAwait(false))
        {
            yield return HandleResult(_serializer.Deserialize<ResultBase>(HandleResponse(response).Payload));
        }
    }

    public async IAsyncEnumerable<int> GetRange(int start, int count, TimeSpan delay, CancellationToken cancellation = default)
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
