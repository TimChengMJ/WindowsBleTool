using BleCore;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class ScanPage : Page
{
    public ScanPage() => InitializeComponent();
    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger) { }
    public void SetDevMode(bool isDev) { }
}
