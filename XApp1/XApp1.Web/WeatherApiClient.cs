using Nerdbank.MessagePack;
using System.Runtime.CompilerServices;
using System.Text.Json;
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
        var request = new GetForecastRequest() { RngSeed = 0, Count = 7 };
        byte[] requestBytes = _serializer.Serialize<RequestBase>(request);
        var jsonRequest = new JsonMessage { Payload = requestBytes };
        var httpResponse = await httpClient.PostAsJsonAsync<JsonMessage>("/weatherforecast", jsonRequest, cancellationToken);
        JsonMessage? jsonResponse = await httpResponse.Content.ReadFromJsonAsync<JsonMessage>(cancellationToken);
        ReadOnlyMemory<byte> buffer = jsonResponse?.Payload ?? ReadOnlyMemory<byte>.Empty;
        var result = _serializer.Deserialize<ResultBase>(buffer);
        return GetWeatherData(result).ToArray();
    }
}

public class JsonMessage
{
    public byte[]? Payload { get; set; }
}
