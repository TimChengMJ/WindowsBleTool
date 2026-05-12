using System.Text;

namespace BleCore;

public static class AdDataParser
{
    private static readonly Dictionary<ushort, string> KnownCompanyIds = new()
    {
        { 0x004C, "Apple" },
        { 0x0059, "Nordic Semiconductor" },
        { 0x0006, "Microsoft" },
        { 0x0077, "Google" },
        { 0x0157, "Samsung" },
        { 0x0087, "Garmin" },
        { 0x010F, "Tile" },
        { 0x0133, "Xiaomi" },
    };

    public static List<ParsedAdField> Parse(Dictionary<ushort, byte[]> adStructures)
    {
        var fields = new List<ParsedAdField>();
        foreach (var (adType, data) in adStructures)
        {
            var field = new ParsedAdField { AdType = adType, RawData = data };
            switch (adType)
            {
                case 0x01:
                    field.Name = "Flags";
                    field.Description = $"{(data.Length > 0 ? $"0x{data[0]:X2}" : "N/A")} {FormatFlags(data.Length > 0 ? data[0] : (byte)0)}";
                    break;
                case 0x02:
                case 0x03:
                    field.Name = adType == 0x02 ? "Incomplete 16-bit UUIDs" : "Complete 16-bit UUIDs";
                    field.Description = string.Join(", ", Enumerate16BitUuids(data).Select(u => $"0x{u:X4}"));
                    break;
                case 0x04:
                case 0x05:
                    field.Name = adType == 0x04 ? "Incomplete 32-bit UUIDs" : "Complete 32-bit UUIDs";
                    field.Description = string.Join(", ", Enumerate32BitUuids(data).Select(u => $"0x{u:X8}"));
                    break;
                case 0x06:
                case 0x07:
                    field.Name = adType == 0x06 ? "Incomplete 128-bit UUIDs" : "Complete 128-bit UUIDs";
                    field.Description = string.Join(", ", Enumerate128BitUuids(data).Select(g => g.ToString("D")));
                    break;
                case 0x08:
                case 0x09:
                    field.Name = adType == 0x08 ? "Shortened Local Name" : "Complete Local Name";
                    field.Description = $"\"{Encoding.UTF8.GetString(data)}\"";
                    break;
                case 0x0A:
                    field.Name = "TX Power Level";
                    field.Description = data.Length > 0 ? $"{data[0]} dBm" : "N/A";
                    break;
                case 0x16:
                    if (data.Length >= 2)
                    {
                        var svcUuid16 = BitConverter.ToUInt16(data, 0);
                        var svcData = data.AsSpan(2).ToArray();
                        field.Name = "Service Data";
                        field.Description = $"UUID: 0x{svcUuid16:X4} · Data: {BitConverter.ToString(svcData)}";
                    }
                    else { field.Name = $"AdType: 0x{adType:X2}"; field.Description = BitConverter.ToString(data); }
                    break;
                case 0xFF:
                    if (data.Length >= 2)
                    {
                        var companyId = BitConverter.ToUInt16(data, 0);
                        var mfrData = data.AsSpan(2).ToArray();
                        var companyName = KnownCompanyIds.TryGetValue(companyId, out var cn) ? cn : "Unknown";
                        field.Name = "Manufacturer Data";
                        field.Description = $"Company ID: 0x{companyId:X4} ({companyName}) · Data: {BitConverter.ToString(mfrData)}";
                    }
                    else { field.Name = $"AdType: 0x{adType:X2}"; field.Description = BitConverter.ToString(data); }
                    break;
                default:
                    field.Name = $"AdType: 0x{adType:X2}";
                    field.Description = BitConverter.ToString(data);
                    break;
            }
            fields.Add(field);
        }
        return fields;
    }

    private static List<ushort> Enumerate16BitUuids(byte[] data)
    {
        var uuids = new List<ushort>();
        for (int i = 0; i <= data.Length - 2; i += 2)
            uuids.Add(BitConverter.ToUInt16(data, i));
        return uuids;
    }

    private static List<uint> Enumerate32BitUuids(byte[] data)
    {
        var uuids = new List<uint>();
        for (int i = 0; i <= data.Length - 4; i += 4)
            uuids.Add(BitConverter.ToUInt32(data, i));
        return uuids;
    }

    private static List<Guid> Enumerate128BitUuids(byte[] data)
    {
        var uuids = new List<Guid>();
        for (int i = 0; i <= data.Length - 16; i += 16)
            uuids.Add(new Guid(data.AsSpan(i, 16)));
        return uuids;
    }

    private static string FormatFlags(byte flags)
    {
        var parts = new List<string>();
        if ((flags & 0x01) != 0) parts.Add("LE Limited Discoverable");
        if ((flags & 0x02) != 0) parts.Add("LE General Discoverable");
        if ((flags & 0x04) != 0) parts.Add("BR/EDR Not Supported");
        if ((flags & 0x08) != 0) parts.Add("Simultaneous LE + BR/EDR (Controller)");
        if ((flags & 0x10) != 0) parts.Add("Simultaneous LE + BR/EDR (Host)");
        return parts.Count > 0 ? string.Join(" | ", parts) : "None";
    }
}

public class ParsedAdField
{
    public ushort AdType { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte[] RawData { get; init; } = Array.Empty<byte>();
}
