using System.Text;

namespace AcpNet.Process;

internal sealed class RecordingTextWriter(TextWriter inner, AcpTranscriptRecorder recorder) : TextWriter
{
    public override Encoding Encoding => inner.Encoding;

    public override void WriteLine(string? value)
    {
        if (value is not null)
        {
            recorder.Out(value);
        }

        inner.WriteLine(value);
        inner.Flush();
    }

    public override async Task WriteLineAsync(string? value)
    {
        if (value is not null)
        {
            recorder.Out(value);
        }

        await inner.WriteLineAsync(value);
        await inner.FlushAsync();
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
