using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace BleTool.Shared.Scripting;

public class ScriptEngine : IDisposable
{
    private readonly V8ScriptEngine _engine;
    private readonly BleScriptApi _bleApi;
    private bool _running;
    private CancellationTokenSource? _cts;

    public event Action<string>? ConsoleOutput;
    public event Action<string>? ErrorOutput;
    public bool IsRunning => _running;

    public ScriptEngine(global::BleCore.BleScanner scanner)
    {
        _bleApi = new BleScriptApi(scanner);
        _engine = new V8ScriptEngine();

        _engine.AddHostObject("ble", _bleApi);

        _engine.AddHostObject("console", new
        {
            log = new Action<object>(msg =>
            {
                var text = msg?.ToString() ?? "undefined";
                ConsoleOutput?.Invoke(text);
            }),
            error = new Action<object>(msg =>
            {
                var text = msg?.ToString() ?? "undefined";
                ErrorOutput?.Invoke($"[ERROR] {text}");
            }),
            warn = new Action<object>(msg =>
            {
                var text = msg?.ToString() ?? "undefined";
                ConsoleOutput?.Invoke($"[WARN] {text}");
            })
        });

        _engine.Execute(@"
            const _timers = new Map();
            let _timerId = 0;
            this.host = this;
        ");
    }

    public async Task<string> ExecuteAsync(string script, CancellationToken cancellationToken = default)
    {
        _running = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            var wrapped = $@"
                (async () => {{
                    try {{
                        {script}
                    }} catch (e) {{
                        console.error('Script error: ' + e.message);
                    }}
                }})();
            ";

            var result = _engine.Evaluate(wrapped);
            if (result is Task task)
                await task.WaitAsync(_cts.Token);
            else
                await Task.CompletedTask;

            return "Script completed.";
        }
        catch (OperationCanceledException)
        {
            return "Script cancelled.";
        }
        catch (Exception ex)
        {
            ErrorOutput?.Invoke($"Script execution failed: {ex.Message}");
            return $"Script failed: {ex.Message}";
        }
        finally
        {
            _running = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Stop()
    {
        _engine.Interrupt();
        _cts?.Cancel();
        _running = false;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _engine.Dispose();
    }
}
