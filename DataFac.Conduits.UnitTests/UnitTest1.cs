using DataFac.Conduits.Testing;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Testing.Weather;
using Xunit;

namespace DataFac.Conduits.UnitTests;

public class UnitTest1
{
    [Fact]
    public async Task DisposedServiceShouldThrow()
    {
        var timeProvider = new FakeTimeProvider();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        FakeConduitServer conduitServer = new FakeConduitServer(new ConduitServer(timeProvider, new WeatherServer(new WeatherService())));
        try
        {
            await using var client = new WeatherClient(new ConduitClient(new FakeConduitClient(conduitServer, timeProvider)));
            var weather = await client.GetWeatherForecast(1, 7, cts.Token);
            weather.ShouldNotBeNull();
            weather.Batch.Length.ShouldBe(7);
        }
        finally
        {
            await conduitServer.DisposeAsync();
        }
        // note: conduitServer is disposed
        // repeat
        {
            await using var client = new WeatherClient(new ConduitClient(new FakeConduitClient(conduitServer, timeProvider)));
            var ex = await Assert.ThrowsAsync<ObjectDisposedException>(
                         async () =>
                         {
                             var weather = await client.GetWeatherForecast(1, 7, cts.Token);
                         });
            ex.Message.ShouldStartWith("Cannot access a disposed object.");
        }
    }

    [Fact]
    public async Task ServiceCallsAreRepeatable()
    {
        var timeProvider = new FakeTimeProvider();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await using var conduitServer = new FakeConduitServer(new ConduitServer(timeProvider, new WeatherServer(new WeatherService())));
        {
            await using var client = new WeatherClient(new ConduitClient(new FakeConduitClient(conduitServer, timeProvider)));
            var weather = await client.GetWeatherForecast(1, 7, cts.Token);
            weather.ShouldNotBeNull();
            weather.Batch.Length.ShouldBe(7);
        }
        // repeat
        {
            await using var client = new WeatherClient(new ConduitClient(new FakeConduitClient(conduitServer, timeProvider)));
            var weather = await client.GetWeatherForecast(1, 7, cts.Token);
            weather.ShouldNotBeNull();
            weather.Batch.Length.ShouldBe(7);
        }
    }

}