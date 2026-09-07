using Nerdbank.MessagePack;
using System.Runtime.CompilerServices;
using Testing.Weather;

namespace XApp1.Web;

public class WeatherApiClient(HttpClient httpClient)
{
    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

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

    public async Task<WeatherData[]> GetWeatherAsyncEnum(CancellationToken cancellationToken = default)
    {
        //string json = await httpClient.GetStringAsync("/weatherforecast", cancellationToken);
        JsonMessage? message = await httpClient.GetFromJsonAsync<JsonMessage>("/weatherforecast", cancellationToken);
        ReadOnlyMemory<byte> buffer = message?.Payload ?? ReadOnlyMemory<byte>.Empty;
        var result = _serializer.Deserialize<ResultBase>(buffer);
        return GetWeatherData(result).ToArray();
    }
}

//public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}

public class JsonMessage
{
    public byte[]? Payload { get; set; }
}
