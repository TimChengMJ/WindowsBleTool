using System.Text;

namespace BleTool.Shared.DataFormat;

public static class DataFormatter
{
    public static string Format(byte[] data, DataFormatType format)
    {
        if (data.Length == 0) return "(empty)";

        return format switch
        {
            DataFormatType.Hex => BitConverter.ToString(data).Replace("-", " "),
            DataFormatType.Decimal => string.Join(" ", data.Select(b => b.ToString())),
            DataFormatType.Binary => string.Join(" ", data.Select(b =>
                Convert.ToString(b, 2).PadLeft(8, '0'))),
            DataFormatType.Utf8 => TryDecodeUtf8(data),
            DataFormatType.Base64 => Convert.ToBase64String(data),
            _ => BitConverter.ToString(data).Replace("-", " ")
        };
    }

    public static string[] FormatAll(byte[] data)
    {
        return new[]
        {
            Format(data, DataFormatType.Hex),
            Format(data, DataFormatType.Decimal),
            Format(data, DataFormatType.Binary),
            Format(data, DataFormatType.Utf8),
            Format(data, DataFormatType.Base64)
        };
    }

    private static string TryDecodeUtf8(byte[] data)
    {
        try
        {
            var str = Encoding.UTF8.GetString(data);
            if (str.Any(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t'))
                return "(contains control characters)";
            return str;
        }
        catch
        {
            return "(binary data)";
        }
    }

    public static byte[] ParseInput(string input, DataFormatType format)
    {
        return format switch
        {
            DataFormatType.Hex => ParseHex(input),
            DataFormatType.Utf8 => Encoding.UTF8.GetBytes(input),
            DataFormatType.Base64 => Convert.FromBase64String(input),
            DataFormatType.Decimal => ParseDecimal(input),
            DataFormatType.Binary => ParseBinary(input),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }

    private static byte[] ParseHex(string input)
    {
        var cleaned = input.Replace(" ", "").Replace("-", "");
        if (cleaned.Length % 2 != 0)
            throw new FormatException("Hex string must have an even number of characters");
        var bytes = new byte[cleaned.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
        return bytes;
    }

    private static byte[] ParseDecimal(string input)
    {
        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => byte.Parse(s))
            .ToArray();
    }

    private static byte[] ParseBinary(string input)
    {
        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Convert.ToByte(s, 2))
            .ToArray();
    }
}
