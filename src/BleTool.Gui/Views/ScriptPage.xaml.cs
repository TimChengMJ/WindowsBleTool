using BleCore;
using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class ScriptPage : Page
{
    private readonly ScriptViewModel _vm = new();

    public ScriptPage()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
        => _vm.Initialize(adapter, logger);

    public void SetDevMode(bool isDev) { }
}
