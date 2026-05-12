// src/BleCore/DeviceFilter.cs
using System.Text.RegularExpressions;

namespace BleCore;

public class DeviceFilterRule
{
    public FilterType Type { get; init; }
    public FilterOperator Operator { get; init; }
    public string Value { get; init; } = string.Empty;
}

public enum FilterType
{
    Rssi, DeviceName, AdvertisedUuid, MacAddress, ManufacturerId, RawData, AddressType,
}

public enum FilterOperator
{
    Equal, GreaterThanOrEqual, LessThanOrEqual, Contains, NotContains, Regex,
}

public static class DeviceFilter
{
    public static bool MatchesAll(BleDeviceInfo device, IReadOnlyList<DeviceFilterRule> rules)
        => rules.All(r => MatchesRule(device, r));

    private static bool MatchesRule(BleDeviceInfo device, DeviceFilterRule rule)
    {
        return rule.Type switch
        {
            FilterType.Rssi => MatchRssi(device.Rssi, rule.Operator, rule.Value),
            FilterType.DeviceName => MatchDeviceName(device.Name, rule.Operator, rule.Value),
            FilterType.AdvertisedUuid => MatchUuid(device.AdStructures, rule.Value),
            FilterType.MacAddress => MatchMac(device.Address, rule.Operator, rule.Value),
            FilterType.ManufacturerId => MatchManufacturer(device.AdStructures, rule.Value),
            FilterType.RawData => MatchRawData(device.RawAdvertisement, rule.Value),
            FilterType.AddressType => string.Equals(device.AddressType, rule.Value, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private static bool MatchRssi(int rssi, FilterOperator op, string value)
    {
        if (!int.TryParse(value, out var threshold)) return false;
        return op switch
        {
            FilterOperator.GreaterThanOrEqual => rssi >= threshold,
            FilterOperator.LessThanOrEqual => rssi <= threshold,
            _ => false,
        };
    }

    private static bool MatchDeviceName(string name, FilterOperator op, string pattern)
    {
        return op switch
        {
            FilterOperator.Contains => name.Contains(pattern, StringComparison.OrdinalIgnoreCase),
            FilterOperator.NotContains => !name.Contains(pattern, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Equal => string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Regex => Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase),
            _ => false,
        };
    }

    private static bool MatchUuid(Dictionary<ushort, byte[]> adStructures, string uuid)
    {
        ushort[] uuidTypes = { 0x02, 0x03, 0x06, 0x07 };
        if (!Guid.TryParse(uuid, out var targetGuid))
        {
            if (ushort.TryParse(uuid, System.Globalization.NumberStyles.HexNumber, null, out var shortUuid))
                targetGuid = new Guid($"0000{shortUuid:X4}-0000-1000-8000-00805F9B34FB");
            else return false;
        }

        foreach (var type in uuidTypes)
        {
            if (adStructures.TryGetValue(type, out var data))
            {
                for (int i = 0; i <= data.Length - 16; i++)
                {
                    var guid = new Guid(data.AsSpan(i, 16));
                    if (guid == targetGuid) return true;
                }
            }
        }
        return false;
    }

    private static bool MatchMac(string address, FilterOperator op, string pattern)
    {
        pattern = pattern.Replace("*", ".*").Replace(":", "\\:");
        return op switch
        {
            FilterOperator.Contains => Regex.IsMatch(address, pattern, RegexOptions.IgnoreCase),
            FilterOperator.Equal => string.Equals(address, pattern, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Regex => Regex.IsMatch(address, pattern, RegexOptions.IgnoreCase),
            _ => false,
        };
    }

    private static bool MatchManufacturer(Dictionary<ushort, byte[]> adStructures, string companyId)
    {
        if (!adStructures.TryGetValue(0xFF, out var data) || data.Length < 2) return false;
        if (!ushort.TryParse(companyId, System.Globalization.NumberStyles.HexNumber, null, out var targetId))
            return false;
        var actualId = BitConverter.ToUInt16(data, 0);
        return actualId == targetId;
    }

    private static bool MatchRawData(byte[] rawAd, string hexPattern)
    {
        var pattern = HexToBytes(hexPattern);
        if (pattern == null) return false;
        return IndexOf(rawAd, pattern) >= 0;
    }

    private static byte[]? HexToBytes(string hex)
    {
        hex = hex.Replace(" ", "").Replace("-", "");
        if (hex.Length % 2 != 0) return null;
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            if (!byte.TryParse(hex.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
                return null;
            bytes[i] = b;
        }
        return bytes;
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j]) { match = false; break; }
            }
            if (match) return i;
        }
        return -1;
    }
}
