using BleCore;
using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger) { }
    public void SetDevMode(bool isDev) { }
}
