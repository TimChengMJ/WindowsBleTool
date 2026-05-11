using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace BleTool.Gui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private bool _isDeveloperMode = true;

    [ObservableProperty]
    private string _connectionStatus = "未连接";

    [ObservableProperty]
    private int _connectedDeviceCount;
}
