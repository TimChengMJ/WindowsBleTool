using System.Collections.ObjectModel;
using BleCore;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace BleTool.Gui.ViewModels;

public partial class ScriptEditorViewModel : ObservableObject
{
    private readonly BleScanner _scanner;

    [ObservableProperty] private string _scriptCode = "// 编写 BLE 脚本...\n";
    [ObservableProperty] private ObservableCollection<string> _consoleOutput = new();
    [ObservableProperty] private bool _isRunning;

    public ScriptEditorViewModel(BleScanner scanner)
    {
        _scanner = scanner;
    }

    [RelayCommand]
    private async Task RunScript()
    {
        IsRunning = true;
        ConsoleOutput.Clear();
        var engine = new global::BleTool.Shared.Scripting.ScriptEngine(_scanner);
        engine.ConsoleOutput += msg =>
            DispatcherQueue.TryEnqueue(() => ConsoleOutput.Add(msg));
        engine.ErrorOutput += msg =>
            DispatcherQueue.TryEnqueue(() => ConsoleOutput.Add($"[ERROR] {msg}"));

        try
        {
            await engine.ExecuteAsync(ScriptCode);
        }
        finally
        {
            engine.Dispose();
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void StopScript() { }

    public Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; set; } =
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
}
