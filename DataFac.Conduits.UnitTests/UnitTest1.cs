using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DataFac.Conduits.Testing;
using Shouldly;

namespace DataFac.Conduits.UnitTests;

public class UnitTest1
{
    [Fact]
    public async Task DisposedServiceShouldThrow()
    {
        var timeProvider = new FakeTimeProvider();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        FakeConduitServer conduitServer;
        await using (conduitServer = new FakeConduitServer(new ConduitServer(timeProvider, new WeatherServer(new WeatherService(), timeProvider)), timeProvider))
        {
            await using var conduitClient = new FakeConduitClient(conduitServer, timeProvider);
            await using var client = new WeatherClient(conduitClient, true);
            var weather = await client.GetWeather("Brisbane", cts.Token);
            weather.ShouldNotBeNull();
            weather.Tag.ShouldBe(WeatherTag.NotFound);
        }
        // note: conduitServer is disposed
        // repeat
        {
            await using var conduitClient = new FakeConduitClient(conduitServer, timeProvider);
            await using var client = new WeatherClient(conduitClient, false);
            var ex = await Assert.ThrowsAsync<ObjectDisposedException>(
                         async () =>
                         {
                             var weather = await client.GetWeather("Brisbane", cts.Token);
                         });
            ex.Message.ShouldStartWith("Cannot access a disposed object.");
        }
    }

    [Fact]
    public async Task ServiceCallsAreRepeatable()
    {
        var timeProvider = new FakeTimeProvider();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var conduitServer = new FakeConduitServer(new ConduitServer(timeProvider, new WeatherServer(new WeatherService(), timeProvider)), timeProvider);
        {
            await using var conduitClient = new FakeConduitClient(conduitServer, timeProvider);
            await using var client = new WeatherClient(conduitClient);
            await client.UpdateWeather(new WeatherData(WeatherTag.WeatherData, "Brisbane", 31, timeProvider.GetUtcNow().UtcDateTime), cts.Token);
            var weather = await client.GetWeather("Brisbane", cts.Token);
            weather.TemperatureC.ShouldBe(31.0D);
        }
        // repeat
        {
            await using var client = new WeatherClient(new FakeConduitClient(conduitServer, timeProvider));
            var weather = await client.GetWeather("Brisbane", cts.Token);
            weather.TemperatureC.ShouldBe(31.0D);
        }
    }

    [Fact]
    public async Task StreamingServiceCalls()
    {
        var ct = TestContext.Current.CancellationToken;
        var timeProvider = new FakeTimeProvider();
        var cts1 = Debugger.IsAttached
            ? new CancellationTokenSource(TimeSpan.FromSeconds(30))
            : new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await using var conduitServer = new FakeConduitServer(new ConduitServer(timeProvider, new WeatherServer(new WeatherService(), timeProvider)), timeProvider);
        {
            await using var client = new WeatherClient(new FakeConduitClient(conduitServer, timeProvider));
            List<WeatherData> responses = new List<WeatherData>();
            Exception? fault = null;
            var subscriber = Task.Run(async () =>
            {
                try
                {
                    await foreach (var response in client.GetWeatherStream("Brisbane", cts1.Token))
                    {
                        responses.Add(response);
                    }
                }
                catch (TaskCanceledException)
                {
                    // expected
                    fault = null;
                }
                catch (OperationCanceledException)
                {
                    // expected
                    fault = null;
                }
                catch (Exception ex)
                {
                    fault = ex;
                }
            }, ct);
            var publisher = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await client.UpdateWeather(new WeatherData(WeatherTag.WeatherData, "Brisbane", 31, timeProvider.GetUtcNow().UtcDateTime), cts1.Token);
                await client.UpdateWeather(new WeatherData(WeatherTag.WeatherData, "Brisbane", 32, timeProvider.GetUtcNow().UtcDateTime), cts1.Token);
            }, ct);
            await Task.WhenAll(subscriber, publisher);
            fault.ShouldBeNull();
            responses.Count.ShouldBe(2);
            responses[0].TemperatureC.ShouldBe(31.0D);
            responses[1].TemperatureC.ShouldBe(32.0D);
        }
        // repeat
        {
            var cts2 = Debugger.IsAttached
                ? new CancellationTokenSource(TimeSpan.FromSeconds(30))
                : new CancellationTokenSource(TimeSpan.FromSeconds(5));

            await using var client = new WeatherClient(new FakeConduitClient(conduitServer, timeProvider));
            List<WeatherData> responses = new List<WeatherData>();
            Exception? fault = null;
            try
            {
                await foreach (var response in client.GetWeatherStream("Brisbane", cts2.Token))
                {
                    responses.Add(response);
                }
            }
            catch (TaskCanceledException)
            {
                // expected
                fault = null;
            }
            catch (OperationCanceledException)
            {
                // expected
                fault = null;
            }
            catch (Exception ex)
            {
                fault = ex;
            }
            fault.ShouldBeNull();
            responses.Count.ShouldBe(1);
            responses[0].TemperatureC.ShouldBe(32.0D);
        }
    }
}