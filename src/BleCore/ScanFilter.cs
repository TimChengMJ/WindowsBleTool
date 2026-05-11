using BleCore.Models;

namespace BleCore;

public enum FilterLogic
{
    And,
    Or
}

public enum FilterType
{
    Rssi,
    DeviceName,
    AdvertisedUuid,
    MacAddress,
    ManufacturerId,
    RawData,
    AddressType
}

public enum NameMatchMode
{
    Contains,
    Regex,
    Exact,
    Exclude
}

public enum AddressTypeFilter
{
    Public,
    Random,
    Both
}

public class ScanFilter
{
    public FilterType Type { get; init; }
    public bool IsActive { get; set; } = true;

    // RSSI
    public bool RssiGreaterOrEqual { get; init; } = true;
    public short RssiThreshold { get; init; } = -70;

    // Device Name
    public NameMatchMode NameMatchMode { get; init; } = NameMatchMode.Contains;
    public string NamePattern { get; init; } = string.Empty;

    // UUID
    public Guid UuidFilter { get; init; }

    // MAC address
    public string MacAddressPattern { get; init; } = string.Empty;

    // Manufacturer
    public ushort ManufacturerId { get; init; }

    // Raw data (hex string)
    public string RawDataPattern { get; init; } = string.Empty;

    // Address type
    public AddressTypeFilter AddressTypeFilterValue { get; init; } = AddressTypeFilter.Both;
}

public static class ScanFilterEvaluator
{
    public static bool Matches(BleDeviceInfo device, IReadOnlyList<ScanFilter> filters, FilterLogic logic)
    {
        if (filters.Count == 0) return true;

        return logic == FilterLogic.And
            ? filters.Where(f => f.IsActive).All(f => Evaluate(device, f))
            : filters.Where(f => f.IsActive).Any(f => Evaluate(device, f));
    }

    public static string? GetMismatchReason(BleDeviceInfo device, IReadOnlyList<ScanFilter> filters, FilterLogic logic)
    {
        if (Matches(device, filters, logic)) return null;

        if (logic == FilterLogic.And)
        {
            foreach (var f in filters.Where(f => f.IsActive))
            {
                if (!Evaluate(device, f))
                    return $"{f.Type} 不匹配";
            }
        }
        return "无规则匹配";
    }

    private static bool Evaluate(BleDeviceInfo device, ScanFilter filter)
    {
        return filter.Type switch
        {
            FilterType.Rssi => filter.RssiGreaterOrEqual
                ? device.Rssi >= filter.RssiThreshold
                : device.Rssi <= filter.RssiThreshold,
            FilterType.DeviceName => MatchName(device.Name, filter.NameMatchMode, filter.NamePattern),
            FilterType.AdvertisedUuid => device.AdvertisementData?.Records
                .Where(r => r.AdType is 0x03 or 0x02)
                .Any(r => ContainsUuid(r.Data, filter.UuidFilter)) ?? false,
            FilterType.MacAddress => MatchMacAddress(device.AddressString, filter.MacAddressPattern),
            FilterType.ManufacturerId => device.AdvertisementData?.Records
                .Where(r => r.AdType == 0xFF)
                .Any(r => r.Data.Length >= 2 && BitConverter.ToUInt16(r.Data, 0) == filter.ManufacturerId) ?? false,
            FilterType.RawData => false,
            FilterType.AddressType => true,
            _ => true
        };
    }

    private static bool MatchName(string name, NameMatchMode mode, string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return true;
        if (string.IsNullOrEmpty(name)) return mode == NameMatchMode.Exclude;

        return mode switch
        {
            NameMatchMode.Contains => name.Contains(pattern, StringComparison.OrdinalIgnoreCase),
            NameMatchMode.Exact => name.Equals(pattern, StringComparison.OrdinalIgnoreCase),
            NameMatchMode.Exclude => !name.Contains(pattern, StringComparison.OrdinalIgnoreCase),
            NameMatchMode.Regex => System.Text.RegularExpressions.Regex.IsMatch(name, pattern),
            _ => true
        };
    }

    private static bool MatchMacAddress(string address, string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return true;
        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(address, regexPattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static bool ContainsUuid(byte[] data, Guid uuid)
    {
        var uuidBytes = uuid.ToByteArray();
        for (int i = 0; i <= data.Length - 2; i += 2)
        {
            if (data[i] == uuidBytes[0] && data[i + 1] == uuidBytes[1])
                return true;
        }
        return false;
    }
}
