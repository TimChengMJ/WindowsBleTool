using System.Text.Json;

namespace BleCore;

public class SessionLogger
{
    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new();

    public void Log(string category, string message)
    {
        lock (_lock)
        {
            _entries.Add(new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Category = category,
                Message = message,
            });
        }
    }

    public void LogError(string category, string message)
    {
        lock (_lock)
        {
            _entries.Add(new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Category = category,
                Message = message,
                IsError = true,
            });
        }
    }

    public IReadOnlyList<LogEntry> Entries
    {
        get { lock (_lock) return _entries.ToList(); }
    }

    public string ExportAsText()
    {
        lock (_lock)
        {
            return string.Join(Environment.NewLine,
                _entries.Select(e => $"[{e.Timestamp:HH:mm:ss.fff}] [{e.Category}] {(e.IsError ? "ERROR: " : "")}{e.Message}"));
        }
    }

    public string ExportAsJson()
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }
}

public class LogEntry
{
    public DateTimeOffset Timestamp { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsError { get; init; }
}
