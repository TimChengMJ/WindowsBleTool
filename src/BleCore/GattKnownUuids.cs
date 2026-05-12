namespace BleCore;

public enum DataFormat { Hex, Decimal, Binary, Utf8, Base64 }

public static class GattKnownUuids
{
    private static readonly Dictionary<string, (string Name, DataFormat SuggestedFormat)> Registry
        = new(StringComparer.OrdinalIgnoreCase)
    {
        { "00001800-0000-1000-8000-00805F9B34FB", ("Generic Access", DataFormat.Hex) },
        { "00001801-0000-1000-8000-00805F9B34FB", ("Generic Attribute", DataFormat.Hex) },
        { "0000180A-0000-1000-8000-00805F9B34FB", ("Device Information", DataFormat.Hex) },
        { "0000180D-0000-1000-8000-00805F9B34FB", ("Heart Rate", DataFormat.Hex) },
        { "0000180F-0000-1000-8000-00805F9B34FB", ("Battery Service", DataFormat.Hex) },
        { "00001809-0000-1000-8000-00805F9B34FB", ("Health Thermometer", DataFormat.Hex) },
        { "00001810-0000-1000-8000-00805F9B34FB", ("Blood Pressure", DataFormat.Hex) },
        { "00002A00-0000-1000-8000-00805F9B34FB", ("Device Name", DataFormat.Utf8) },
        { "00002A01-0000-1000-8000-00805F9B34FB", ("Appearance", DataFormat.Hex) },
        { "00002A19-0000-1000-8000-00805F9B34FB", ("Battery Level", DataFormat.Decimal) },
        { "00002A37-0000-1000-8000-00805F9B34FB", ("Heart Rate Measurement", DataFormat.Hex) },
        { "00002A38-0000-1000-8000-00805F9B34FB", ("Body Sensor Location", DataFormat.Decimal) },
        { "00002A39-0000-1000-8000-00805F9B34FB", ("Heart Rate Control Point", DataFormat.Hex) },
        { "00002A6E-0000-1000-8000-00805F9B34FB", ("Temperature Measurement", DataFormat.Hex) },
        { "00002A29-0000-1000-8000-00805F9B34FB", ("Manufacturer Name String", DataFormat.Utf8) },
        { "00002A24-0000-1000-8000-00805F9B34FB", ("Model Number String", DataFormat.Utf8) },
        { "00002A25-0000-1000-8000-00805F9B34FB", ("Serial Number String", DataFormat.Utf8) },
        { "00002A26-0000-1000-8000-00805F9B34FB", ("Firmware Revision String", DataFormat.Utf8) },
        { "00002A27-0000-1000-8000-00805F9B34FB", ("Hardware Revision String", DataFormat.Utf8) },
        { "00002A28-0000-1000-8000-00805F9B34FB", ("Software Revision String", DataFormat.Utf8) },
    };

    public static string GetName(string uuid)
        => Registry.TryGetValue(uuid, out var entry) ? entry.Name : uuid;

    public static DataFormat GetSuggestedFormat(string uuid)
        => Registry.TryGetValue(uuid, out var entry) ? entry.SuggestedFormat : DataFormat.Hex;
}
