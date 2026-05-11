using System.Text.Json;

namespace BleTool.Shared.Logging;

public class SessionLogger
{
    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new();

    public event Action<LogEntry>? EntryAdded;
    public IReadOnlyList<LogEntry> Entries { get { lock(_lock) return _entries.ToList(); } }

    public void Log(LogCategory category, string message)
    {
        var entry = new LogEntry { Category = category, Message = message };
        lock (_lock) _entries.Add(entry);
        EntryAdded?.Invoke(entry);
    }

    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }

    public string ExportText()
    {
        lock (_lock) return string.Join(Environment.NewLine, _entries.Select(e => e.ToString()));
    }

    public string ExportJson()
    {
        lock (_lock)
        {
            var items = _entries.Select(e => new
            {
                timestamp = e.Timestamp.ToString("O"),
                category = e.Category.ToString(),
                message = e.Message
            });
            return JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
