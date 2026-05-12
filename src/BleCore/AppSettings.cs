using System.Text.Json;

namespace BleCore;

public class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WindowsBleTool", "settings.json");

    public int DefaultRssiThreshold { get; set; } = -70;
    public int LogMaxEntries { get; set; } = 10_000;
    public string ScriptTemplatePath { get; set; } = string.Empty;
    public DataFormat PreferredDataFormat { get; set; } = DataFormat.Hex;
    public string PreferredHexSeparator { get; set; } = " ";
    public List<HistoryDevice> RecentDevices { get; set; } = new();
    public List<SavedFilterPreset> FilterPresets { get; set; } = new();

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new();
            }
        }
        catch { }
        return new();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}

public class HistoryDevice
{
    public string Address { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset LastConnected { get; init; }
}

public class SavedFilterPreset
{
    public string Name { get; set; } = string.Empty;
    public List<DeviceFilterRule> Rules { get; set; } = new();
}
