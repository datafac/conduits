using DataFac.Conduits;
using Nerdbank.MessagePack;
using PolyType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Testing.Weather;

[GenerateShape]
[DerivedTypeShape(typeof(GetWeatherRequest), Tag = 1)]
[DerivedTypeShape(typeof(GetForecastRequest), Tag = 2)]
public abstract partial class RequestBase { }

[GenerateShape]
public sealed partial class GetWeatherRequest : RequestBase { }

[GenerateShape]
public sealed partial class GetForecastRequest : RequestBase
{
    [Key(1)] public int Count { get; set; }
}

[GenerateShape]
[DerivedTypeShape(typeof(ErrorResult), Tag = 1)]
[DerivedTypeShape(typeof(WeatherData), Tag = 2)]
public abstract partial class ResultBase { }

[GenerateShape]
public sealed partial class WeatherData : ResultBase
{
    [Key(1)] public long DateTimeUtc { get; set; }
    [Key(2)] public int TemperatureC { get; set; }
    [Key(3)] public string? Summary { get; set; }
}

[GenerateShape]
public sealed partial class ErrorResult : ResultBase
{
    [Key(1)] public ErrorCode Code { get; set; }

    [Key(2)] public string Message { get; set; } = string.Empty;
}
