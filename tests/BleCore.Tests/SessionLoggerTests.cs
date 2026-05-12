using BleCore;

namespace BleCore.Tests;

[TestClass]
public class SessionLoggerTests
{
    [TestMethod]
    public void Log_AddsEntry()
    {
        var logger = new SessionLogger();
        logger.Log("SCAN", "Started scanning");
        Assert.AreEqual(1, logger.Entries.Count);
        Assert.AreEqual("SCAN", logger.Entries[0].Category);
    }

    [TestMethod]
    public void ExportAsText_FormatsCorrectly()
    {
        var logger = new SessionLogger();
        logger.Log("CONNECT", "Connected to device");
        var text = logger.ExportAsText();
        Assert.IsTrue(text.Contains("[CONNECT]"));
        Assert.IsTrue(text.Contains("Connected to device"));
    }

    [TestMethod]
    public void ExportAsJson_ProducesValidJson()
    {
        var logger = new SessionLogger();
        logger.Log("SCAN", "Test");
        var json = logger.ExportAsJson();
        Assert.IsTrue(json.Contains("SCAN"));
        Assert.IsTrue(json.Contains("Test"));
    }
}
