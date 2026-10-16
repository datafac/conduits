using System;

namespace DataFac.Conduits;

public readonly struct AppInfo
{
    public string Name { get; }
    public Version Version { get; }
    public override string ToString() => $"Name={Name};Version={Version}";

    private static string ValidInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(input));
        if (input.Length > 200) throw new ArgumentException("Value cannot be longer than 200 characters.", nameof(input));
        Span<char> output = stackalloc char[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            char ch = input[i];
            if (Char.IsControl(ch) || ch == '{' || ch == '}' || ch == ';' || ch == '=')
            {
                throw new ArgumentException("Value cannot contain '{', '}', ';', '=' or control characters.", nameof(input));
            }
            else
            {
                output[i] = ch;
            }
        }
#if NET8_0_OR_GREATER
        return new string(output);
#else
        return new string(output.ToArray());
#endif
    }

    public static AppInfo ParseFrom(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(input));
        // for each key-value pair delimited by ';', extract the key and value delimited by '='
        ReadOnlySpan<char> remaining = input.AsSpan();
        string? name = null;
        Version? version = null;
        // find first ';'
        int delimPos = remaining.IndexOf(';');
        while (remaining.Length > 0)
        {
            ReadOnlySpan<char> pair;
            if (delimPos == -1)
            {
                pair = remaining;
                remaining = ReadOnlySpan<char>.Empty;
            }
            else
            {
                pair = remaining.Slice(0, delimPos);
                remaining = remaining.Slice(delimPos + 1);
                // find next ';'
                delimPos = remaining.IndexOf(';');
            }
            // find '='
            int eqPos = pair.IndexOf('=');
            if (eqPos >= 0)
            {
                string key = pair.Slice(0, eqPos).ToString();
                string value = pair.Slice(eqPos + 1).ToString();
                switch (key.ToLowerInvariant())
                {
                    case "name":
                        name = ValidInput(value);
                        break;
                    case "version":
                        version = Version.Parse(value);
                        break;
                    default:
                        // ignore unknown keys
                        break;
                }
            }
        }

        // check required values are present
        if (name is null) throw new ArgumentException("Input does not contain a value for 'name'", nameof(input));
        if (version is null) throw new ArgumentException("Input does not contain a value for 'version'", nameof(input));

        return new AppInfo(name, version);
    }

    public AppInfo(string name, Version version) : this()
    {
        Name = ValidInput(name);
        Version = version;
    }
}
