using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class GattBrowserPage : Page
{
    public GattBrowserViewModel ViewModel { get; } = new();

    public GattBrowserPage()
    {
        this.InitializeComponent();
    }
}
