using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public class WeatherServer : IAppConduit
{
    private static readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IAsyncWeather _handler;

    public WeatherServer(IAsyncWeather handler)
    {
        _handler = handler;
    }

    public async ValueTask<AppResponse> UnaryRequest(AppRequest appRequest, CancellationToken cancellation = default)
    {
        RequestBase? request = _serializer.Deserialize<RequestBase>(appRequest.Payload);
        ResultBase result;
        try
        {
            result = request switch
            {
                null => new ErrorResult { Code = ErrorCode.InvalidData, Message = "Failed to deserialize request" },
                GetForecastRequest fr => await _handler.GetWeatherForecast(fr.RngSeed, fr.Count, cancellation),
                _ => new ErrorResult { Code = ErrorCode.InvalidData, Message = $"Unknown request type: {request.GetType().Name}" }
            };
        }
        catch (Exception e)
        {
            result = new ErrorResult { Code = ErrorCode.OtherException, Message = e.Message };
        }
        return new AppResponse(_serializer.Serialize<ResultBase>(result));
    }

    public async IAsyncEnumerable<AppResponse> ServerStream(AppRequest requestBytes, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var result = new AppResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.InvalidOp, Message = "Server streaming is not supported" }));
        yield return result;
    }

    public async ValueTask<AppResponse> ClientStream(IAsyncEnumerable<AppRequest> requests, CancellationToken cancellation = default)
    {
        var result = new AppResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.InvalidOp, Message = "Client streaming is not supported" }));
        return result;
    }

    public async IAsyncEnumerable<AppResponse> DuplexStream(IAsyncEnumerable<AppRequest> requests, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var result = new AppResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.InvalidOp, Message = "Duplex streaming is not supported" }));
        yield return result;
    }

}