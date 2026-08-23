using System;
using System.Text;

namespace DataFac.Conduits.UnitTests
{
    public sealed record WeatherData
    {
        public WeatherTag Tag { get; }
        public string Location { get; }
        public double TemperatureC { get; }
        public DateTime LastUpdatedUtc { get; }

        private readonly static WeatherData _empty = new WeatherData(WeatherTag.Empty, string.Empty);
        public static WeatherData Empty => _empty;

        public WeatherData(WeatherTag tag, string location)
        {
            Tag = tag;
            Location = location;
            TemperatureC = default;
            LastUpdatedUtc = default;
        }

        public WeatherData(WeatherTag tag, string location, double temperatureC, DateTime lastUpdatedUtc)
        {
            Tag = tag;
            Location = location;
            TemperatureC = temperatureC;
            LastUpdatedUtc = lastUpdatedUtc;
        }

        public ReadOnlyMemory<byte> ToMemory()
        {
            return Encoding.UTF8.GetBytes($"{Tag}|{Location}|{TemperatureC:R}|{LastUpdatedUtc:O}");
        }

        public static WeatherData FromSpan(ReadOnlySpan<byte> input)
        {
            if (input.IsEmpty) return Empty;
            string decoded = Encoding.UTF8.GetString(input.ToArray());
            var fields = decoded.Split('|');
            return new WeatherData(
                (WeatherTag)Enum.Parse(typeof(WeatherTag), fields[0]),
                fields[1],
                double.Parse(fields[2]),
                DateTime.Parse(fields[3]));
        }
    }
}