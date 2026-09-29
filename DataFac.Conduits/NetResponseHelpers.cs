using System;
using System.Text;

namespace DataFac.Conduits;

public static class NetResponseHelpers
{
    /// <summary>
    /// Encodes a message string as UTF-8 bytes to a payload.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ReadOnlyMemory<byte> EncodeMsg(this string message)
    {
        if (message.Length == 0) return ReadOnlyMemory<byte>.Empty;
        return Encoding.UTF8.GetBytes(message);
    }

    /// <summary>
    /// Decodes a UTF-8 encoded string from a payload.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static string DecodeMsg(this ReadOnlyMemory<byte> payload)
    {
        if (payload.Length == 0) return string.Empty;
        // limit stack alloc to 1K
        if (payload.Length > 1024)
        {
            return Encoding.UTF8.GetString(payload.ToArray());
        }
#if NET8_0_OR_GREATER
        Span<char> chars = stackalloc char[payload.Length / 2]; // max 1K bytes
        if (Encoding.UTF8.TryGetChars(payload.Span, chars, out int charsWritten))
        {
            return new string(chars.Slice(0, charsWritten));
        }
        else
        {
            return Encoding.UTF8.GetString(payload.ToArray());
        }
#else
        return Encoding.UTF8.GetString(payload.ToArray());
#endif
    }

}