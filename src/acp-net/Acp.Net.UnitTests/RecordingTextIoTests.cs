using AcpNet.Process;
using Xunit;

namespace Acp.Net.UnitTests;

public sealed class RecordingTextIoTests
{
    [Fact]
    public async Task RecordingTextWriter_RecordsAndFlushesLine()
    {
        var recorder = new AcpTranscriptRecorder();
        await using var stream = new MemoryStream();
        await using var streamWriter = new StreamWriter(stream) { AutoFlush = false };
        using var writer = new RecordingTextWriter(streamWriter, recorder);

        await writer.WriteLineAsync("""{"method":"initialize"}""");

        Assert.True(stream.Length > 0);
        Assert.Contains("initialize", recorder.Snapshot()[0]);
    }

    [Fact]
    public async Task RecordingTextReader_RecordsReadLine()
    {
        var recorder = new AcpTranscriptRecorder();
        using var reader = new RecordingTextReader(new StringReader("""{"method":"session/update"}""" + Environment.NewLine), recorder);

        var line = await reader.ReadLineAsync();

        Assert.NotNull(line);
        Assert.Contains("session/update", recorder.Snapshot()[0]);
    }
}
