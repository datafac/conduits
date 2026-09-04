using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.UnitTests;

internal sealed class WeatherServer : IUserChannel
{
    private readonly TimeProvider _timeProvider;
    public TimeProvider TimeProvider => _timeProvider;

    private readonly IWeatherService _server;

    public string ServerName => ThisAssembly.AssemblyName;
    public string ServerVersion => ThisAssembly.AssemblyFileVersion;

    public WeatherServer(IWeatherService server, TimeProvider? timeProvider)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async ValueTask DisposeAsync()
    {
        // nothing to dispose yet
    }

    private async ValueTask<UserResponse> ProcessRequest(UserRequest request, CancellationToken token)
    {
        var weatherRequest = WeatherData.FromSpan(request.Payload.Span);
        switch (weatherRequest.Tag)
        {
            case WeatherTag.GetWeatherData:
                {
                    WeatherData weatherResponse = await _server.GetWeather(weatherRequest.Location, token);
                    return new UserResponse(weatherResponse.ToMemory());
                }
            case WeatherTag.WeatherData:
                {
                    await _server.UpdateWeather(weatherRequest, token);
                    return new UserResponse(new WeatherData(WeatherTag.OK, weatherRequest.Location).ToMemory());
                }
            default:
                return new UserResponse(WeatherData.Empty.ToMemory());
        }
    }

    public async ValueTask<UserResponse> UnaryRequest(UserRequest request, CancellationToken cancellation = default)
    {
        return await ProcessRequest(request, cancellation);
    }

    public async IAsyncEnumerable<UserResponse> ServerStream(UserRequest request, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var weatherRequest = WeatherData.FromSpan(request.Payload.Span);
        switch (weatherRequest.Tag)
        {
            case WeatherTag.StreamDn_WeatherFeed:
                {
                    await foreach (WeatherData response in _server.GetWeatherStream(weatherRequest.Location, cancellation))
                    {
                        yield return new UserResponse(response.ToMemory());
                    }
                }
                break;
            default:
                break;
        }
    }

    public async ValueTask<UserResponse> ClientStream(IAsyncEnumerable<UserRequest> requests, CancellationToken cancellation = default)
    {
        var pushTask = Task.Run(async () =>
        {
            await foreach (var request in requests)
            {
                var weatherRequest = WeatherData.FromSpan(request.Payload.Span);
                switch (weatherRequest.Tag)
                {
                    case WeatherTag.WeatherData:
                        {
                            await _server.UpdateWeather(weatherRequest, cancellation);
                        }
                        break;
                    default:
                        break;
                }
            }
        });
        await Task.WhenAll(pushTask);
        return new UserResponse(WeatherData.Empty.ToMemory());
    }

    public async IAsyncEnumerable<UserResponse> DuplexStream(IAsyncEnumerable<UserRequest> requests, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var pushTask = Task.Run(async () =>
        {
            await foreach (var request in requests)
            {
                var weatherRequest = WeatherData.FromSpan(request.Payload.Span);
                switch (weatherRequest.Tag)
                {
                    case WeatherTag.WeatherData:
                        {
                            await _server.UpdateWeather(weatherRequest, cancellation);
                        }
                        break;
                    default:
                        break;
                }
            }
        });
        var location = string.Empty; // all
        await foreach (WeatherData response in _server.GetWeatherStream(location, cancellation))
        {
            yield return new UserResponse(response.ToMemory());
        }
        await Task.WhenAll(pushTask);
    }
}
