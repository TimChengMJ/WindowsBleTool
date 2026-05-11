namespace BleCore.Models;

public class AdvertisementData
{
    public IReadOnlyList<AdvertisementRecord> Records { get; init; } = Array.Empty<AdvertisementRecord>();
    public DateTimeOffset Timestamp { get; init; }
}

public class AdvertisementRecord
{
    public byte AdType { get; init; }
    public string AdTypeName => AdType switch
    {
        0x01 => "Flags",
        0x02 => "Incomplete 16-bit UUIDs",
        0x03 => "Complete 16-bit UUIDs",
        0x08 => "Shortened Local Name",
        0x09 => "Complete Local Name",
        0x0A => "Tx Power Level",
        0x16 => "Service Data",
        0xFF => "Manufacturer Specific Data",
        _ => $"Unknown (0x{AdType:X2})"
    };
    public byte[] Data { get; init; } = Array.Empty<byte>();
}
