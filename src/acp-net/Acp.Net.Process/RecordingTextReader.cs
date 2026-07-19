using System.Text;

namespace AcpNet.Process;

internal sealed class RecordingTextReader(TextReader inner, AcpTranscriptRecorder recorder) : TextReader
{
    // Characters consumed through the char-level API that have not yet formed a
    // complete line. Flushed to the transcript on '\n' and on Dispose so that
    // ReadToEnd/Read consumers are recorded just like ReadLine consumers.
    readonly StringBuilder pending = new();

    public override int Peek() => inner.Peek();

    public override int Read()
    {
        var ch = inner.Read();
        if (ch >= 0)
        {
            RecordChars([(char)ch], 0, 1);
        }

        return ch;
    }

    public override int Read(char[] buffer, int index, int count)
    {
        var read = inner.Read(buffer, index, count);
        RecordChars(buffer, index, read);
        return read;
    }

    public override async Task<int> ReadAsync(char[] buffer, int index, int count)
    {
        var read = await inner.ReadAsync(buffer, index, count);
        RecordChars(buffer, index, read);
        return read;
    }

    public override string ReadToEnd()
    {
        var text = inner.ReadToEnd();
        RecordChars(text.ToCharArray(), 0, text.Length);
        return text;
    }

    public override async Task<string> ReadToEndAsync()
    {
        var text = await inner.ReadToEndAsync();
        RecordChars(text.ToCharArray(), 0, text.Length);
        return text;
    }

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

    void RecordChars(char[] buffer, int index, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var ch = buffer[index + i];
            if (ch == '\n')
            {
                FlushPendingLine();
            }
            else
            {
                pending.Append(ch);
            }
        }
    }

    void FlushPendingLine()
    {
        if (pending.Length > 0 && pending[^1] == '\r')
        {
            pending.Length--;
        }

        recorder.In(pending.ToString());
        pending.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (pending.Length > 0)
            {
                FlushPendingLine();
            }

            inner.Dispose();
        }

        base.Dispose(disposing);
    }
}
