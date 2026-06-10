using System.Text.Json;
using System.Text.Json.Serialization;

namespace AcpNet.Process;

public sealed class AcpTranscriptRecorder
{
    static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    readonly object gate = new();
    readonly List<string> entries = [];

    static AcpTranscriptRecorder()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public int Count
    {
        get
        {
            lock (gate)
            {
                return entries.Count;
            }
        }
    }

    public void Out(string line) => Add("out", line);

    public void In(string line) => Add("in", line);

    public void Error(string line) => Add("stderr", line);

    public void Event(string name, object? value = null)
    {
        var payload = JsonSerializer.Serialize(new
        {
            ts = DateTimeOffset.UtcNow,
            direction = "event",
            name,
            value
        }, SerializerOptions);

        lock (gate)
        {
            entries.Add(payload);
        }
    }

    public string[] Snapshot()
    {
        lock (gate)
        {
            return entries.ToArray();
        }
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllLines(path, Snapshot());
    }

    void Add(string direction, string line)
    {
        object payload;
        try
        {
            payload = new
            {
                ts = DateTimeOffset.UtcNow,
                direction,
                json = JsonDocument.Parse(line).RootElement.Clone()
            };
        }
        catch (JsonException)
        {
            payload = new
            {
                ts = DateTimeOffset.UtcNow,
                direction,
                line
            };
        }

        lock (gate)
        {
            entries.Add(JsonSerializer.Serialize(payload, SerializerOptions));
        }
    }
}
