using DataFac.Conduits;
using DataFac.Conduits.HttpCommon;
using Nerdbank.MessagePack;
using Testing.Weather;

namespace XApp1.Web;

// todo move to HttpConduitClient
internal sealed class HttpConduitClient : INetConduit
{
    private readonly HttpClient _httpClient;
    public HttpConduitClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async ValueTask<NetResponse> UnaryRequest(NetRequest request, DateTime? deadline, CancellationToken cancellation = default)
    {
        var jsonRequest = new JsonRequest { DeadlineUtc = deadline?.Ticks, Payload = request.Payload.ToArray() };
        var httpResponse = await _httpClient.PostAsJsonAsync<JsonRequest>(EndpointPath.UnaryRequest, jsonRequest, cancellation);
        JsonResponse? jsonResponse = await httpResponse.Content.ReadFromJsonAsync<JsonResponse>(cancellation);
        if (jsonResponse is null) return new NetResponse(ControlCode.Invalid, ReadOnlyMemory<byte>.Empty); // todo encode error message
        ReadOnlyMemory<byte> payload = jsonResponse.Payload ?? ReadOnlyMemory<byte>.Empty;
        return new NetResponse((ControlCode)jsonResponse.ControlCode, payload);
    }

    public ValueTask<NetResponse> ClientStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadline, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<NetResponse> DuplexStream(IAsyncEnumerable<NetRequest> requests, DateTime? deadline, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<NetResponse> ServerStream(NetRequest request, DateTime? deadline, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }
}

public class WeatherApiClient
{
    private readonly MessagePackSerializer _serializer = new MessagePackSerializer();

    private readonly IAppConduit _channel;

    public WeatherApiClient(HttpClient httpClient)
    {
        _channel = new ProtocolClient(new HttpConduitClient(httpClient));
        var weatherSvc = new WeatherClient(_channel); // todo
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

    public async Task<WeatherData[]> GetWeatherAsyncEnum(CancellationToken cancellationToken = default)
    {
        var request = new GetForecastRequest() { RngSeed = 0, Count = 7 };
        var userRequest = new AppRequest(_serializer.Serialize<RequestBase>(request));
        var userResponse = await _channel.UnaryRequest(userRequest, cancellationToken);
        var result = _serializer.Deserialize<ResultBase>(userResponse.Payload);
        return GetWeatherData(result).ToArray();
    }
}

