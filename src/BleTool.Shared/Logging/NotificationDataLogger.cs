using System.Text;

namespace BleTool.Shared.Logging;

public class NotificationDataLogger
{
    private readonly List<NotificationRecord> _records = new();
    private readonly object _lock = new();
    private int _maxRecords = 10000;

    public event Action<NotificationRecord>? RecordAdded;
    public int RecordCount { get { lock(_lock) return _records.Count; } }
    public int MaxRecords
    {
        get => _maxRecords;
        set { _maxRecords = Math.Max(100, value); }
    }

    public void Record(string deviceName, string characteristicUuid, string hexValue, string parsedValue = "")
    {
        var record = new NotificationRecord
        {
            DeviceName = deviceName,
            CharacteristicUuid = characteristicUuid,
            HexValue = hexValue,
            ParsedValue = parsedValue
        };

        lock (_lock)
        {
            _records.Add(record);
            while (_records.Count > _maxRecords)
                _records.RemoveAt(0);
        }
        RecordAdded?.Invoke(record);
    }

    public void Clear()
    {
        lock (_lock) _records.Clear();
    }

    public IReadOnlyList<NotificationRecord> GetRecords()
    {
        lock (_lock) return _records.ToList();
    }

    public string ExportCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Device,CharacteristicUUID,HexValue,ParsedValue");
        lock (_lock)
        {
            foreach (var r in _records)
            {
                sb.AppendLine(
                    $"\"{r.Timestamp:O}\",\"{r.DeviceName}\",\"{r.CharacteristicUuid}\",\"{r.HexValue}\",\"{r.ParsedValue}\"");
            }
        }
        return sb.ToString();
    }
}
