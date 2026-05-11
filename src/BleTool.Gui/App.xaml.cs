using BleCore;
using BleTool.Shared.Logging;
using Microsoft.UI.Xaml;

namespace BleTool.Gui;

public partial class App : Application
{
    public static BleScanner Scanner { get; } = new();
    public static SessionLogger SessionLogger { get; } = new();
    public static NotificationDataLogger NotificationLogger { get; } = new();

    private Window? _window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
