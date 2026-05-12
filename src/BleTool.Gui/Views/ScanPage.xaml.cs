using BleCore;
using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class ScanPage : Page
{
    private readonly ScanViewModel _vm = new();

    public ScanPage()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
        => _vm.Initialize(adapter, logger);

    public void SetDevMode(bool isDev) => _vm.SetDevMode(isDev);
}
