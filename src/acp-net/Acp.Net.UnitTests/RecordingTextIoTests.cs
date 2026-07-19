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

    [Fact]
    public async Task RecordingTextReader_ReadToEndAsync_ReturnsContentAndRecordsLines()
    {
        // Regression for issue #2: ReadToEnd* used to inherit TextReader's default
        // implementation and silently return an empty string.
        var recorder = new AcpTranscriptRecorder();
        using var reader = new RecordingTextReader(new StringReader("first\r\nsecond\nlast-no-newline"), recorder);

        var text = await reader.ReadToEndAsync();
        reader.Dispose();

        Assert.Equal("first\r\nsecond\nlast-no-newline", text);
        var entries = recorder.Snapshot();
        Assert.Equal(3, entries.Length);
        Assert.Contains("first", entries[0]);
        Assert.Contains("second", entries[1]);
        Assert.Contains("last-no-newline", entries[2]);
    }

    [Fact]
    public void RecordingTextReader_ReadToEnd_ReturnsContent()
    {
        var recorder = new AcpTranscriptRecorder();
        using var reader = new RecordingTextReader(new StringReader("score 94.5\n"), recorder);

        Assert.Equal("score 94.5\n", reader.ReadToEnd());
        Assert.Contains("score 94.5", recorder.Snapshot()[0]);
    }

    [Fact]
    public void RecordingTextReader_CharReads_ForwardAndRecord()
    {
        var recorder = new AcpTranscriptRecorder();
        using var reader = new RecordingTextReader(new StringReader("ab\ncd"), recorder);

        Assert.Equal('a', (char)reader.Peek());
        Assert.Equal('a', (char)reader.Read());

        var buffer = new char[8];
        var read = reader.ReadBlock(buffer, 0, 8);

        Assert.Equal("b\ncd", new string(buffer, 0, read));
        reader.Dispose();

        var entries = recorder.Snapshot();
        Assert.Equal(2, entries.Length);
        Assert.Contains("ab", entries[0]);
        Assert.Contains("cd", entries[1]);
    }
}
