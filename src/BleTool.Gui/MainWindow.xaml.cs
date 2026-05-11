using BleTool.Gui.ViewModels;
using BleTool.Gui.Views;
using BleTool.Shared.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        this.InitializeComponent();

        App.Scanner.ScanStatusChanged += status =>
            DispatcherQueue.TryEnqueue(() => ViewModel.ConnectionStatus = status);
        App.SessionLogger.EntryAdded += entry =>
            DispatcherQueue.TryEnqueue(() =>
            {
                if (entry.Category == LogCategory.Connect)
                    ViewModel.ConnectedDeviceCount++;
            });

        ScanTab.Click += (_, _) => ContentFrame.Navigate(typeof(ScanPage));
        GattTab.Click += (_, _) => ContentFrame.Navigate(typeof(GattBrowserPage));
        ScriptTab.Click += (_, _) => ContentFrame.Navigate(typeof(ScriptEditorPage));
        SettingsTab.Click += (_, _) => ContentFrame.Navigate(typeof(SettingsPage));

        ContentFrame.Navigate(typeof(ScanPage));
    }
}
