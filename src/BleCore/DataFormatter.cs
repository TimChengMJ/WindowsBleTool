using System.Text;

namespace BleCore;

public static class DataFormatter
{
    public static string Format(byte[] data, DataFormat format, string separator = " ")
    {
        return format switch
        {
            DataFormat.Hex => BitConverter.ToString(data).Replace("-", separator),
            DataFormat.Decimal => string.Join(separator, data.Select(b => b.ToString())),
            DataFormat.Binary => string.Join(separator, data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))),
            DataFormat.Utf8 => Encoding.UTF8.GetString(data),
            DataFormat.Base64 => Convert.ToBase64String(data),
            _ => BitConverter.ToString(data).Replace("-", separator),
        };
    }

    public static DataFormat SuggestFormat(string characteristicUuid)
    {
        var guid = new Guid(characteristicUuid);
        return guid.ToString("D").ToUpper() switch
        {
            "00002A00-0000-1000-8000-00805F9B34FB" => DataFormat.Utf8,
            "00002A19-0000-1000-8000-00805F9B34FB" => DataFormat.Decimal,
            "00002A37-0000-1000-8000-00805F9B34FB" => DataFormat.Hex,
            "00002A6E-0000-1000-8000-00805F9B34FB" => DataFormat.Decimal,
            _ => DataFormat.Hex,
        };
    }
}
