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
    private static readonly AppResponse errorDeserializationFailure
        = new AppResponse(_serializer.Serialize<ResultBase>(
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

    private async Task<BatchResult> GetForecastAsArray(GetForecastRequest fr, CancellationToken cancellation)
    {
        var all = await _handler.GetForecast(fr.RngSeed, fr.Count, cancellation).ToArrayAsync();
        return new BatchResult { Results = all };
    }

    public async ValueTask<AppResponse> UnaryRequest(AppRequest requestBytes, CancellationToken cancellation = default)
    {
        RequestBase? request = _serializer.Deserialize<RequestBase>(requestBytes.Payload);
        if (request is null) return errorDeserializationFailure;
        ResultBase result;
        try
        {
            result = request switch
            {
                GetWeatherRequest wr => await _handler.GetWeather(wr.RngSeed, cancellation),
                GetForecastRequest fr => await GetForecastAsArray(fr, cancellation),
                _ => new ErrorResult { Code = ErrorCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" }
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
        RequestBase? request = _serializer.Deserialize<RequestBase>(requestBytes.Payload);
        if (request is null)
        {
            yield return errorDeserializationFailure;
            yield break;
        }
        else if (request is GetWeatherRequest wr)
        {
            var weather = await _handler.GetWeather(wr.RngSeed, cancellation);
            yield return new AppResponse(_serializer.Serialize<ResultBase>(weather));
        }
        else if (request is GetForecastRequest fr)
        {
            await foreach (var response in _handler.GetForecast(fr.RngSeed, fr.Count, cancellation).ConfigureAwait(false))
            {
                yield return new AppResponse(_serializer.Serialize<ResultBase>(response));
            }
        }
        else
        {
            yield return new AppResponse(_serializer.Serialize<ResultBase>(new ErrorResult { Code = ErrorCode.UnsupportedRequestType, Message = $"Unknown request type: {request.GetType().Name}" }));
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