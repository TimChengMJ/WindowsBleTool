// src/BleCore/ScriptEngine.cs
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace BleCore;

public class ScriptEngine : IDisposable
{
    private readonly V8ScriptEngine _engine;
    private readonly BleJsApi _bleApi;
    private readonly CancellationTokenSource _cancelSource = new();

    public ScriptEngine(IBleAdapter adapter, Action<string, bool>? onOutput = null)
    {
        _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion
            | V8ScriptEngineFlags.EnableDynamicModuleImports);

        var output = onOutput ?? ((_, _) => { });
        _engine.AddHostObject("console", new HostConsole(output));

        _bleApi = new BleJsApi(adapter, (msg, isErr) => output(msg, isErr));
        _engine.AddHostObject("ble", _bleApi);
    }

    public Task<string?> RunAsync(string scriptCode)
    {
        try
        {
            _engine.Execute(scriptCode);
            return Task.FromResult<string?>(null);
        }
        catch (ScriptEngineException ex)
        {
            return Task.FromResult<string?>($"Script error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Task.FromResult<string?>($"Error: {ex.Message}");
        }
    }

    public void Cancel()
    {
        _cancelSource.Cancel();
        _engine.Interrupt();
    }

    public void Dispose()
    {
        _cancelSource.Cancel();
        _engine.Dispose();
    }
}

public class HostConsole
{
    private readonly Action<string, bool> _output;

    public HostConsole(Action<string, bool> output) => _output = output;

    public void log(string message) => _output(message, false);
    public void error(string message) => _output(message, true);
    public void warn(string message) => _output(message, true);
}
