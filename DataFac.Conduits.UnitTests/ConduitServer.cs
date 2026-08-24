using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DataFac.Conduits.UnitTests
{
    internal sealed class ConduitServer : IConduitServer, IDisposable
    {
        private readonly IWeatherService _server;

        public string ServerName => ThisAssembly.AssemblyName;
        public string ServerVersion => ThisAssembly.AssemblyFileVersion;

        public ConduitServer(IWeatherService server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public void Dispose()
        {
            // nothing to dispose yet
        }

        public async ValueTask<ReadOnlyMemory<byte>> SimpleUnaryCall(ReadOnlyMemory<byte> request, CallContext context)
        {
            return await ProcessRequest(request, context.Token);
        }

        private async ValueTask<ReadOnlyMemory<byte>> ProcessRequest(ReadOnlyMemory<byte> request, CancellationToken token)
        {
            var weatherRequest = WeatherData.FromSpan(request.Span);
            switch (weatherRequest.Tag)
            {
                case WeatherTag.GetWeatherData:
                    {
                        WeatherData weatherResponse = await _server.GetWeather(weatherRequest.Location, token);
                        return weatherResponse.ToMemory();
                    }
                case WeatherTag.WeatherData:
                    {
                        await _server.UpdateWeather(weatherRequest, token);
                        return new WeatherData(WeatherTag.OK, weatherRequest.Location).ToMemory();
                    }
                default:
                    return WeatherData.Empty.ToMemory();
            }
        }

        public async IAsyncEnumerable<ReadOnlyMemory<byte>> ServerStream(ReadOnlyMemory<byte> request, CallContext context)
        {
            var weatherRequest = WeatherData.FromSpan(request.Span);
            switch (weatherRequest.Tag)
            {
                case WeatherTag.StreamDn_WeatherFeed:
                    {
                        await foreach (WeatherData response in _server.GetWeatherStream(weatherRequest.Location, context.Token))
                        {
                            ReadOnlyMemory<byte> payload = response.ToMemory();
                            yield return payload;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public async ValueTask<ReadOnlyMemory<byte>> ClientStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, CallContext context)
        {
            var pushTask = Task.Run(async () =>
            {
                await foreach (var request in requests)
                {
                    var weatherRequest = WeatherData.FromSpan(request.Span);
                    switch (weatherRequest.Tag)
                    {
                        case WeatherTag.WeatherData:
                            {
                                await _server.UpdateWeather(weatherRequest, context.Token);
                            }
                            break;
                        default:
                            break;
                    }
                }
            });
            await Task.WhenAll(pushTask);
            return WeatherData.Empty.ToMemory();
        }

        public async IAsyncEnumerable<ReadOnlyMemory<byte>> DuplexStream(IAsyncEnumerable<ReadOnlyMemory<byte>> requests, DateTime? deadlineUtc = null, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var pushTask = Task.Run(async () =>
            {
                await foreach (var request in requests)
                {
                    var weatherRequest = WeatherData.FromSpan(request.Span);
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
                yield return response.ToMemory();
            }
            await Task.WhenAll(pushTask);
        }
    }
}

