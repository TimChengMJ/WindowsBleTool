using BleCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BleTool.Gui.Views;

namespace BleTool.Gui;

public sealed partial class MainWindow : Window
{
    private readonly WindowsBleAdapter _adapter = new();
    private readonly SessionLogger _sessionLogger = new();

    private ToggleSwitch _devModeToggle = null!;
    private TabView _mainTabView = null!;
    private ScanPage _scanPage = null!;
    private GattPage _gattPage = null!;
    private ScriptPage _scriptPage = null!;

    public MainWindow()
    {
        InitializeComponent();
        BuildUI();
        Instance = this;
    }

    private void BuildUI()
    {
        var grid = (Grid)Content;

        // Dev mode toggle
        _devModeToggle = new ToggleSwitch
        {
            Header = "开发者模式",
            IsOn = true,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 8, 8, 0),
        };
        _devModeToggle.Toggled += (s, e) =>
        {
            var isDev = _devModeToggle.IsOn;
            _scanPage.SetDevMode(isDev);
            _gattPage.SetDevMode(isDev);
        };
        Grid.SetRow(_devModeToggle, 0);
        grid.Children.Add(_devModeToggle);

        // Create pages
        _scanPage = new ScanPage();
        _scanPage.Initialize(_adapter, _sessionLogger);
        _gattPage = new GattPage();
        _gattPage.Initialize(_adapter, _sessionLogger);
        _scriptPage = new ScriptPage();
        _scriptPage.Initialize(_adapter, _sessionLogger);
        var settingsPage = new SettingsPage();

        // TabView
        _mainTabView = new TabView();
        Grid.SetRow(_mainTabView, 1);
        _mainTabView.TabItems.Add(CreateTab("扫描设备", _scanPage));
        _mainTabView.TabItems.Add(CreateTab("GATT 浏览器", _gattPage));
        _mainTabView.TabItems.Add(CreateTab("脚本编辑器", _scriptPage));
        _mainTabView.TabItems.Add(CreateTab("设置", settingsPage));
        grid.Children.Add(_mainTabView);

        // Status bar
        var statusBar = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.SolidColorBrush)
                Application.Current.Resources["CardBackgroundFillColorDefault"],
            Padding = new Thickness(8, 4, 8, 4),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 16,
                Children =
                {
                    new TextBlock { Text = "就绪", FontSize = 12 },
                    new TextBlock { Text = "0 台设备", FontSize = 12 },
                }
            }
        };
        Grid.SetRow(statusBar, 2);
        grid.Children.Add(statusBar);
    }

    private static TabViewItem CreateTab(string header, Page page)
    {
        var item = new TabViewItem { Header = header, IsClosable = false };
        item.Content = page;
        return item;
    }

    public void NavigateToGattPage(string deviceId, string deviceName)
    {
        _mainTabView.SelectedIndex = 1;
        _ = _gattPage.OpenDevice(deviceId, deviceName);
    }

    public static MainWindow? Instance { get; private set; }
}
