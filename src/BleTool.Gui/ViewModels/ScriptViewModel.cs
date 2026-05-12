using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BleCore;
using System.Collections.ObjectModel;

namespace BleTool.Gui.ViewModels;

public partial class ScriptViewModel : ObservableObject
{
    private WindowsBleAdapter? _adapter;
    private BleCore.ScriptEngine? _engine;

    [ObservableProperty]
    private string _scriptCode = "// 在此编写 BLE 脚本...\n";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "就绪";

    public ObservableCollection<ConsoleLine> ConsoleOutput { get; } = new();

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
        => _adapter = adapter;

    [RelayCommand]
    public async Task RunScript()
    {
        if (_adapter == null || IsRunning) return;
        IsRunning = true;
        StatusText = "运行中...";
        ConsoleOutput.Clear();

        _engine = new BleCore.ScriptEngine(_adapter, (msg, isErr) =>
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
            {
                ConsoleOutput.Add(new ConsoleLine
                {
                    Text = $"[{DateTimeOffset.UtcNow:HH:mm:ss}] {msg}",
                    IsError = isErr
                });
            });
        });

        var error = await _engine.RunAsync(ScriptCode);
        if (error != null)
            ConsoleOutput.Add(new ConsoleLine { Text = error, IsError = true });

        IsRunning = false;
        StatusText = error == null ? "完成" : "错误";
        _engine.Dispose();
    }

    [RelayCommand]
    public void StopScript()
    {
        _engine?.Cancel();
        StatusText = "已停止";
        IsRunning = false;
    }

    public void LoadTemplate(string code) => ScriptCode = code;
}

public class ConsoleLine
{
    public string Text { get; init; } = string.Empty;
    public bool IsError { get; init; }
}
