using DataFac.Conduits;
using DataFac.Conduits.HttpClient;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Testing.Weather;

namespace XApp1.Web;

public class WeatherApiClient
{
    private readonly IAsyncWeather _weatherSvc;

    private readonly HttpClient _httpClient;
    public WeatherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _weatherSvc = new WeatherClient(new ConduitClient(new HttpConduitClient(httpClient), true));
    }

    private static IEnumerable<WeatherData> GetWeatherData(ResultBase? result)
    {
        if (result is WeatherData weather1)
        {
            yield return weather1;
        }
        else if (result is BatchResult batch)
        {
            foreach (var item in batch.Results)
            {
                foreach(var weather2 in GetWeatherData(item))
                {
                    yield return weather2;
                }
            }
        }
        else if (result is ErrorResult error)
        {
            throw new Exception(error.Message);
        }
    }

    public async Task<WeatherData[]> GetWeatherAsyncEnum(CancellationToken cancellation = default)
    {
        var response = await _weatherSvc.GetWeatherForecast(0, 7, cancellation);
        return response.Batch;
    }
}

