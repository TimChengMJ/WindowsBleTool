using System.Text;

namespace BleCore;

public class NotificationDataLogger
{
    private readonly List<NotificationRecord> _records = new();
    private readonly object _lock = new();
    public int MaxRecords { get; set; } = 10_000;

    public void Record(string deviceName, string deviceAddress, string serviceUuid,
        string characteristicUuid, byte[] data, string parsedValue = "")
    {
        lock (_lock)
        {
            if (_records.Count >= MaxRecords)
                _records.RemoveAt(0);

            _records.Add(new NotificationRecord
            {
                Timestamp = DateTimeOffset.UtcNow,
                DeviceName = deviceName,
                DeviceAddress = deviceAddress,
                ServiceUuid = serviceUuid,
                CharacteristicUuid = characteristicUuid,
                HexValue = BitConverter.ToString(data).Replace("-", " "),
                ParsedValue = parsedValue,
            });
        }
    }

    public IReadOnlyList<NotificationRecord> Records
    {
        get { lock (_lock) return _records.ToList(); }
    }

    public string ExportAsCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Device Name,Device Address,Service UUID,Characteristic UUID,Hex Value,Parsed Value");
        lock (_lock)
        {
            foreach (var r in _records)
            {
                sb.AppendLine($"\"{r.Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}\"," +
                    $"\"{r.DeviceName}\",\"{r.DeviceAddress}\"," +
                    $"\"{r.ServiceUuid}\",\"{r.CharacteristicUuid}\"," +
                    $"\"{r.HexValue}\",\"{r.ParsedValue}\"");
            }
        }
        return sb.ToString();
    }

    public void Clear()
    {
        lock (_lock) _records.Clear();
    }
}

public class NotificationRecord
{
    public DateTimeOffset Timestamp { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public string DeviceAddress { get; init; } = string.Empty;
    public string ServiceUuid { get; init; } = string.Empty;
    public string CharacteristicUuid { get; init; } = string.Empty;
    public string HexValue { get; init; } = string.Empty;
    public string ParsedValue { get; init; } = string.Empty;
}
