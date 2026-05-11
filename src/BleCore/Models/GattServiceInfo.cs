namespace BleCore.Models;

public class GattServiceInfo
{
    public Guid Uuid { get; init; }
    public string Name => UuidToName(Uuid);
    public IReadOnlyList<GattCharacteristicInfo> Characteristics { get; init; } = Array.Empty<GattCharacteristicInfo>();

    private static string UuidToName(Guid uuid) => uuid.ToString("D").ToUpper() switch
    {
        "00001800-0000-1000-8000-00805F9B34FB" => "Generic Access (0x1800)",
        "00001801-0000-1000-8000-00805F9B34FB" => "Generic Attribute (0x1801)",
        "0000180A-0000-1000-8000-00805F9B34FB" => "Device Information (0x180A)",
        "0000180D-0000-1000-8000-00805F9B34FB" => "Heart Rate (0x180D)",
        "0000180F-0000-1000-8000-00805F9B34FB" => "Battery Service (0x180F)",
        "00001809-0000-1000-8000-00805F9B34FB" => "Health Thermometer (0x1809)",
        _ => uuid.ToString("D").ToUpper()
    };
}

public class GattCharacteristicInfo
{
    public Guid Uuid { get; init; }
    public string Name => UuidToName(Uuid);
    public string Properties { get; init; } = string.Empty;
    public byte[]? Value { get; init; }
    public IReadOnlyList<GattDescriptorInfo> Descriptors { get; init; } = Array.Empty<GattDescriptorInfo>();

    private static string UuidToName(Guid uuid)
    {
        var shortId = (ushort)(uuid.ToByteArray()[1] << 8 | uuid.ToByteArray()[0]);
        return shortId switch
        {
            0x2A00 => "Device Name",
            0x2A01 => "Appearance",
            0x2A19 => "Battery Level",
            0x2A37 => "Heart Rate Measurement",
            0x2A38 => "Body Sensor Location",
            0x2A39 => "Heart Rate Control Point",
            0x2A29 => "Manufacturer Name String",
            0x2A6E => "Temperature Measurement",
            _ => $"0x{shortId:X4}"
        };
    }
}

public class GattDescriptorInfo
{
    public Guid Uuid { get; init; }
    public byte[]? Value { get; init; }
}
