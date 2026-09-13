using Nerdbank.MessagePack;
using PolyType;
using System;

namespace Testing.Weather;

[GenerateShape]
[DerivedTypeShape(typeof(GetForecastRequest), Tag = 1)]
public abstract partial class RequestBase { }

[GenerateShape]
public sealed partial class GetForecastRequest : RequestBase
{
    [Key(1)] public int RngSeed { get; set; }
    [Key(2)] public int Count { get; set; }
}

[GenerateShape]
[DerivedTypeShape(typeof(ErrorResult), Tag = 1)]
[DerivedTypeShape(typeof(BatchResult), Tag = 2)]
[DerivedTypeShape(typeof(WeatherData), Tag = 3)]
[DerivedTypeShape(typeof(WeatherForecast), Tag = 4)]
public abstract partial class ResultBase { }

[GenerateShape]
public sealed partial class WeatherData : ResultBase
{
    [Key(1)] public long DateTimeUtc { get; set; }
    public DateTime DateTime => new DateTime(DateTimeUtc, DateTimeKind.Utc);
    [Key(2)] public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5.0);
    [Key(3)] public string? Summary { get; set; }
}

[GenerateShape]
public sealed partial class WeatherForecast : ResultBase
{
    [Key(1)] public WeatherData[] Batch { get; set; } = Array.Empty<WeatherData>();
}

[GenerateShape]
public sealed partial class ErrorResult : ResultBase
{
    [Key(1)] public ErrorCode Code { get; set; }

    [Key(2)] public string Message { get; set; } = string.Empty;
}

[GenerateShape]
public sealed partial class BatchResult : ResultBase
{
    [Key(1)] public ResultBase[] Results { get; set; } = Array.Empty<ResultBase>();
}
