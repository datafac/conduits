using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public class WeatherClient : IAsyncWeather
{
    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IUserChannel _userChannel;

    public WeatherClient(IUserChannel userChannel)
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
                ErrorCode.None => result,
                ErrorCode.DeserializationError => throw new InvalidDataException(errorResult.Message),
                ErrorCode.UnsupportedRequestType => throw new NotSupportedException(errorResult.Message),
                ErrorCode.OtherException => throw new Exception(errorResult.Message),
                _ => throw new Exception($"Unknown error code: {errorResult.Code}")
            },
            _ => result
        };
    }

    public async ValueTask<WeatherData> GetWeather(int rngSeed, CancellationToken cancellation = default)
    {
        var request = new UserRequest(_serializer.Serialize<RequestBase>(new GetWeatherRequest() { RngSeed = rngSeed }));
        var response = await _userChannel.UnaryRequest(request, cancellation).ConfigureAwait(false);
        var result = HandleResult(_serializer.Deserialize<ResultBase>(response.Payload));
        return result as WeatherData ?? throw new Exception($"Unexpected result type: {result.GetType().Name}");
    }

    public async IAsyncEnumerable<WeatherData> GetForecast(int rngSeed, int count, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        RequestBase request = new GetForecastRequest() { RngSeed = rngSeed, Count = count };
        ReadOnlyMemory<byte> requestBytes = _serializer.Serialize<RequestBase>(request);
        await foreach (var response in _userChannel.ServerStream(new UserRequest(requestBytes), cancellation).ConfigureAwait(false))
        {
            var result = HandleResult(_serializer.Deserialize<ResultBase>(response.Payload));
            yield return result as WeatherData ?? throw new Exception($"Unexpected result type: {result.GetType().Name}");
        }
    }
}
