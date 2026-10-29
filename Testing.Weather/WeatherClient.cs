using DataFac.Conduits;
using Nerdbank.MessagePack;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

public class WeatherClient : IAsyncWeather
{
    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IAppConduit _appConduit;

    public WeatherClient(IAppConduit userChannel)
    {
        _appConduit = userChannel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_appConduit is IAsyncDisposable disposable)
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
                ErrorCode.InvalidData => throw new InvalidDataException(errorResult.Message),
                ErrorCode.InvalidOp => throw new InvalidOperationException(errorResult.Message),
                ErrorCode.OtherException => throw new Exception(errorResult.Message),
                _ => throw new Exception($"Unknown error code: {errorResult.Code}")
            },
            _ => result
        };
    }

    public async ValueTask<AppInfo> GetAppInfo(CancellationToken cancellation = default) => await _appConduit.GetAppInfo(cancellation);

    public async ValueTask<WeatherForecast> GetWeatherForecast(int rngSeed, int count, CancellationToken cancellation = default)
    {
        RequestBase request = new GetForecastRequest() { RngSeed = rngSeed, Count = count };
        ReadOnlyMemory<byte> requestBytes = _serializer.Serialize<RequestBase>(request);
        var response = await _appConduit.UnaryRequest(new AppRequest(requestBytes), cancellation).ConfigureAwait(false);
        var result = HandleResult(_serializer.Deserialize<ResultBase>(response.Payload));
        return result as WeatherForecast ?? throw new Exception($"Unexpected result type: {result.GetType().Name}");
    }
}
