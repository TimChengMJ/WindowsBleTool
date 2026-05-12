using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BleCore;

namespace BleTool.Gui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings = AppSettings.Load();

    [ObservableProperty]
    private int _defaultRssiThreshold;

    [ObservableProperty]
    private int _logMaxEntries;

    [ObservableProperty]
    private string _hexSeparator = " ";

    public SettingsViewModel()
    {
        DefaultRssiThreshold = _settings.DefaultRssiThreshold;
        LogMaxEntries = _settings.LogMaxEntries;
        HexSeparator = _settings.PreferredHexSeparator;
    }

    [RelayCommand]
    public void Save()
    {
        _settings.DefaultRssiThreshold = DefaultRssiThreshold;
        _settings.LogMaxEntries = LogMaxEntries;
        _settings.PreferredHexSeparator = HexSeparator;
        _settings.Save();
    }
}
