using BleCore;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class GattPage : Page
{
    public GattPage() => InitializeComponent();
    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger) { }
    public void SetDevMode(bool isDev) { }
    public System.Threading.Tasks.Task OpenDevice(string deviceId, string deviceName)
    {
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
