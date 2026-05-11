using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace BleTool.Gui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private int _maxNotificationRecords = 10000;
    [ObservableProperty] private ObservableCollection<string> _filterPresets = new();
    [ObservableProperty] private string? _selectedPreset;

    [RelayCommand]
    private void SaveFilterPreset(string name)
    {
        if (!FilterPresets.Contains(name))
            FilterPresets.Add(name);
    }

    [RelayCommand]
    private void LoadFilterPreset(string name)
    {
        SelectedPreset = name;
    }
}
