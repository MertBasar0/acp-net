using AcpNet.Process;
using Xunit;

namespace Acp.Net.UnitTests;

public sealed class AcpTranscriptRecorderTests
{
    [Fact]
    public void Recorder_CapturesJsonAndPlainText()
    {
        var recorder = new AcpTranscriptRecorder();

        recorder.Out("""{"jsonrpc":"2.0","method":"initialize"}""");
        recorder.Error("stderr line");
        recorder.Event("process.started", new { pid = 123 });

        var snapshot = recorder.Snapshot();

        Assert.Equal(3, snapshot.Length);
        Assert.Contains("\"direction\":\"out\"", snapshot[0]);
        Assert.Contains("\"method\":\"initialize\"", snapshot[0]);
        Assert.Contains("stderr line", snapshot[1]);
        Assert.Contains("process.started", snapshot[2]);
    }

    [Fact]
    public void Recorder_SaveCreatesFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"acp-transcript-{Guid.NewGuid():N}.ndjson");
        var recorder = new AcpTranscriptRecorder();
        recorder.Event("test");

        try
        {
            recorder.Save(path);

            Assert.True(File.Exists(path));
            Assert.NotEmpty(File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
