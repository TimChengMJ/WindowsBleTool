using BleCore;
using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class GattPage : Page
{
    private readonly GattViewModel _vm = new();

    public GattPage()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
        => _vm.Initialize(adapter, logger);

    public async Task OpenDevice(string deviceId, string deviceName)
        => await _vm.ConnectAndDiscover(deviceId, deviceName);

    public void SetDevMode(bool isDev) => _vm.SetDevMode(isDev);
}
