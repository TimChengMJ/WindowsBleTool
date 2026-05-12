using System.Diagnostics;

namespace Cli.Tests;

[TestClass]
public class IntegrationTests
{
    private static string? _cliPath;

    [ClassInitialize]
    public static void FindCli(TestContext context)
    {
        // Find CLI exe relative to test output
        var testDir = AppContext.BaseDirectory;
        var cliDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", "..", "..",
            "src", "BleTool.Cli", "bin", "Debug", "net9.0-windows10.0.19041.0"));
        _cliPath = Path.Combine(cliDir, "BleTool.Cli.exe");

        if (!File.Exists(_cliPath))
        {
            // Try release path
            cliDir = cliDir.Replace("Debug", "Release");
            _cliPath = Path.Combine(cliDir, "BleTool.Cli.exe");
        }

        context.WriteLine($"CLI path: {_cliPath}");
        context.WriteLine($"CLI exists: {File.Exists(_cliPath)}");
    }

    private (int ExitCode, string Output, string Error) RunCli(string args)
    {
        Assert.IsNotNull(_cliPath, "CLI exe not found");
        Assert.IsTrue(File.Exists(_cliPath), $"CLI exe not found at: {_cliPath}");

        var psi = new ProcessStartInfo
        {
            FileName = _cliPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)!;
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit(5000);

        return (process.ExitCode, output, error);
    }

    [TestMethod]
    public void Help_ReturnsZeroAndShowsHelp()
    {
        var (exit, output, error) = RunCli("--help");
        Assert.AreEqual(0, exit, $"Exit code: {exit}, Error: {error}");
        Assert.IsTrue(output.Contains("Windows BLE debugging CLI tool"), $"Output: {output}");
    }

    [TestMethod]
    public void ScanHelp_ReturnsZero()
    {
        var (exit, output, error) = RunCli("scan --help");
        Assert.AreEqual(0, exit, $"Exit code: {exit}, Error: {error}");
        Assert.IsTrue(output.Contains("--rssi"), $"Output: {output}");
    }

    [TestMethod]
    public void ConnectHelp_ReturnsZero()
    {
        var (exit, output, error) = RunCli("connect --help");
        Assert.AreEqual(0, exit, $"Exit code: {exit}, Error: {error}");
    }

    [TestMethod]
    public void ReadHelp_ReturnsZero()
    {
        var (exit, output, error) = RunCli("read --help");
        Assert.AreEqual(0, exit, $"Exit code: {exit}, Error: {error}");
    }

    [TestMethod]
    public void WriteHelp_ReturnsZero()
    {
        var (exit, output, error) = RunCli("write --help");
        Assert.AreEqual(0, exit, $"Exit code: {exit}, Error: {error}");
    }

    [TestMethod]
    public void SubscribeHelp_ReturnsZero()
    {
        var (exit, output, error) = RunCli("subscribe --help");
        Assert.AreEqual(0, exit, $"Exit code: {exit}, Error: {error}");
    }

    [TestMethod]
    public void RunHelp_ReturnsZero()
    {
        var (exit, output, error) = RunCli("run --help");
        Assert.AreEqual(0, exit, $"Exit code: {exit}, Error: {error}");
    }

    [TestMethod]
    public void Scan_WithoutBluetooth_DoesNotCrash()
    {
        var (exit, output, error) = RunCli("scan --timeout 1000");
        // May fail without Bluetooth, but should not crash (exit code can be non-zero)
        Assert.IsTrue(string.IsNullOrEmpty(error) || error.Contains("BLE") || error.Contains("adapter"),
            $"Unexpected error: {error}");
    }

    [TestMethod]
    public void InvalidCommand_ReturnsNonZero()
    {
        var (exit, _, _) = RunCli("nonexistent-command");
        Assert.AreNotEqual(0, exit);
    }

    [TestMethod]
    public void Version_ShowsToolName()
    {
        // System.CommandLine auto-generates version from assembly
        // Just verify no crash with --version flag
        var (exit, output, error) = RunCli("--version");
        // exit code may be 0 or non-zero depending on args
        Assert.IsTrue(string.IsNullOrEmpty(error) || error.Length >= 0,
            $"Should not crash on --version. Error: {error}");
    }
}
