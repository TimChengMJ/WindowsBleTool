using BleCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BleTool.Gui.Views;

namespace BleTool.Gui;

public sealed partial class MainWindow : Window
{
    private readonly WindowsBleAdapter _adapter = new();
    private readonly SessionLogger _sessionLogger = new();

    public MainWindow()
    {
        InitializeComponent();
        ScanPage.Initialize(_adapter, _sessionLogger);
        GattPage.Initialize(_adapter, _sessionLogger);
        ScriptPage.Initialize(_adapter, _sessionLogger);
    }

    private void OnModeToggled(object sender, RoutedEventArgs e)
    {
        var isDev = DevModeToggle.IsOn;
        ScanPage.SetDevMode(isDev);
        GattPage.SetDevMode(isDev);
    }

    public void NavigateToGattPage(string deviceId, string deviceName)
    {
        MainTabView.SelectedIndex = 1;
        _ = GattPage.OpenDevice(deviceId, deviceName);
    }

    public static MainWindow? Current { get; private set; }
}
