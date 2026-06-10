namespace AcpNet.Process;

internal sealed class RecordingTextReader(TextReader inner, AcpTranscriptRecorder recorder) : TextReader
{
    public override string? ReadLine()
    {
        var line = inner.ReadLine();
        if (line is not null)
        {
            recorder.In(line);
        }

        return line;
    }

    public override async Task<string?> ReadLineAsync()
    {
        var line = await inner.ReadLineAsync();
        if (line is not null)
        {
            recorder.In(line);
        }

        return line;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            inner.Dispose();
        }

        base.Dispose(disposing);
    }
}
