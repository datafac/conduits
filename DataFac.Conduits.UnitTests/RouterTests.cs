using DataFac.Conduits.Testing;
using Shouldly;
using System;
using System.Threading.Tasks;
using Testing.Calculator;
using Testing.Weather;
using Xunit;

namespace DataFac.Conduits.UnitTests;

public class RouterTests
{
    [Fact]
    public async Task RouteCalculatorAndWeatherOverSameConduit()
    {
        var timeProvider = new FakeTimeProvider();
        await using var router = new ConduitRouter()
            .WithRoute(1, new ConduitServer(timeProvider, new CalculatorServer(new Calculator()), true), true)
            .WithRoute(2, new ConduitServer(timeProvider, new WeatherServer(new WeatherService()), true), true);
        await using var client1 = new CalculatorClient(new ConduitClient(router, false, 1));
        await using var client2 = new WeatherClient(new ConduitClient(router, false, 2));

        var ct = TestContext.Current.CancellationToken;

        var result1 = await client1.GetAppInfo(ct);
        result1.ToString().ShouldStartWith("Name=Testing.Calculator.Calculator;Version=1.0.");

        var result2 = await client2.GetAppInfo(ct);
        result2.ToString().ShouldStartWith("Name=Testing.Weather.WeatherService;Version=1.0.");
    }

    [Fact]
    public async Task InvalidRouteThrows()
    {
        var timeProvider = new FakeTimeProvider();
        await using var router = new ConduitRouter()
            .WithRoute(1, new ConduitServer(timeProvider, new CalculatorServer(new Calculator()), true), true);
        await using var client1 = new CalculatorClient(new ConduitClient(router, false, 99));

        var cancellation = TestContext.Current.CancellationToken;

        Func<Task> action = async () => await client1.GetAppInfo(cancellation);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(action);
        ex.Message.ShouldBe("Invalid route: 99");
    }
}
