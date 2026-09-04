using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public class WeatherServer : IUserChannel
{
    private static readonly MessagePackSerializer _serializer = new MessagePackSerializer();
    private static readonly UserResponse errorDeserializationFailure
        = new UserResponse(_serializer.Serialize<ResultBase>(
            new ErrorResult
            {
                Code = ErrorCode.DeserializationError,
                Message = "Failed to deserialize request"
            }));

    private readonly IAsyncWeather _handler;

    public WeatherServer(IAsyncWeather handler)
    {
        _handler = handler;
    }

    public async ValueTask<UserResponse> UnaryRequest(UserRequest requestBytes, CancellationToken cancellation = default)
    {
        RequestBase? request = _serializer.Deserialize<RequestBase>(requestBytes.Payload);
        if (request is null) return errorDeserializationFailure;
        ResultBase result;
        try
        {
            result = request switch
            {
                GetWeatherRequest br => await _handler.GetWeather(),
                _ => new ErrorResult { Code = ErrorCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" }
            };
        }
        catch (Exception e)
        {
            result = new ErrorResult { Code = ErrorCode.OtherException, Message = e.Message };
        }
        return new UserResponse(_serializer.Serialize<ResultBase>(result));
    }

    public async IAsyncEnumerable<UserResponse> ServerStream(UserRequest requestBytes, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        RequestBase? request = _serializer.Deserialize<RequestBase>(requestBytes.Payload);
        if (request is null)
        {
            yield return errorDeserializationFailure;
            yield break;
        }
        else if (request is GetForecastRequest rr)
        {
            await foreach (var response in _handler.GetForecast(rr.Count, cancellation).ConfigureAwait(false))
            {
                yield return new UserResponse(_serializer.Serialize<ResultBase>(response));
            }
        }
        else
        {
            yield return new UserResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" }));
        }
    }

    public ValueTask<UserResponse> ClientStream(IAsyncEnumerable<UserRequest> requests, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<UserResponse> DuplexStream(IAsyncEnumerable<UserRequest> requests, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

}