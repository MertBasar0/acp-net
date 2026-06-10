using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AgentClientProtocol;

var mode = args.FirstOrDefault() ?? "both";
var scriptPath = SpikeHelpers.FindMockAgent();
var results = new List<ProbeResult>();

if (mode is "both" or "incumbent")
{
    results.Add(await RunIncumbentAsync(scriptPath));
}

if (mode is "both" or "acpnet")
{
    results.Add(await RunAcpNetStyleAsync(scriptPath));
}

Console.WriteLine();
Console.WriteLine("=== score-summary ===");
foreach (var result in results)
{
    Console.WriteLine($"{result.Name}: protocol={result.ProtocolScore}, streaming={result.StreamingScore}, lifecycle={result.LifecycleScore}, interop={result.InteropScore}, debug={result.DebugScore}");
    foreach (var note in result.Notes)
    {
        Console.WriteLine($"- {note}");
    }
}

static async Task<ProbeResult> RunIncumbentAsync(string scriptPath)
{
    var transcript = new Transcript();
    using var process = SpikeHelpers.StartPythonAgent(scriptPath);
    _ = SpikeHelpers.DrainAsync(process.StandardError, line => transcript.Error(line));

    var client = new ProbeClient(transcript);
    using var connection = new ClientSideConnection(_ => client, process.StandardOutput, new TranscriptTextWriter(process.StandardInput, transcript));

    connection.Open();

    var init = await connection.InitializeAsync(new InitializeRequest
    {
        ProtocolVersion = 1,
        ClientCapabilities = new ClientCapabilities()
    });

    var session = await connection.NewSessionAsync(new NewSessionRequest
    {
        Cwd = Directory.GetCurrentDirectory(),
        McpServers = []
    });

    var prompt = await connection.PromptAsync(new PromptRequest
    {
        SessionId = session.SessionId,
        Prompt = [new TextContentBlock { Text = "Hello from incumbent package" }]
    });

    await connection.CancelAsync(new CancelNotification { SessionId = session.SessionId });

    await SpikeHelpers.StopProcessAsync(process, transcript, TimeSpan.FromMilliseconds(500));

    transcript.Save("incumbent-transcript.ndjson");

    return new ProbeResult(
        "AgentClientProtocol",
        ProtocolScore: init.ProtocolVersion == 1 && session.SessionId.Length > 0 && prompt.StopReason.ToString().Length > 0 ? 2 : 1,
        StreamingScore: client.StreamChunkCount >= 2 ? 2 : 0,
        LifecycleScore: 1,
        InteropScore: 0,
        DebugScore: transcript.Count > 0 ? 1 : 0,
        Notes:
        [
            "NuGet SDK protocol/schema tarafini calistirdi.",
            "Child process baslatma, stderr drain, kill ve path karari uygulama kodunda kaldi.",
            "Transcript icin TextWriter wrapper gibi ek uygulama kodu gerekti."
        ]);
}

static async Task<ProbeResult> RunAcpNetStyleAsync(string scriptPath)
{
    var bridge = new StdioProcessBridge(scriptPath);
    await using var bridgeScope = bridge;

    var init = await bridge.RequestAsync("initialize", new
    {
        protocolVersion = 1,
        clientCapabilities = new { }
    });

    var session = await bridge.RequestAsync("session/new", new
    {
        cwd = Directory.GetCurrentDirectory(),
        mcpServers = Array.Empty<object>()
    });

    using var promptCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
    var prompt = await bridge.RequestAsync("session/prompt", new
    {
        sessionId = session.GetProperty("sessionId").GetString(),
        prompt = new[] { new { type = "text", text = "Hello from Acp.Net style bridge" } }
    }, promptCts.Token);

    await bridge.NotificationAsync("session/cancel", new
    {
        sessionId = session.GetProperty("sessionId").GetString()
    });

    await bridge.StopAsync(TimeSpan.FromMilliseconds(500));
    bridge.Transcript.Save("acpnet-style-transcript.ndjson");

    return new ProbeResult(
        "Acp.Net-style bridge",
        ProtocolScore: init.TryGetProperty("protocolVersion", out var initProtocol) && prompt.TryGetProperty("stopReason", out var promptStopReason) ? 1 : 0,
        StreamingScore: bridge.NotificationCount >= 2 ? 1 : 0,
        LifecycleScore: 2,
        InteropScore: bridge.UsedWslBridge ? 2 : 1,
        DebugScore: bridge.Transcript.Count > 0 ? 2 : 0,
        Notes:
        [
            "Protocol typing zayif; raw JSON ile calisiyor.",
            "Process lifecycle, WSL bridge ve transcript tek yerde toplandi.",
            "Bu yaklasim SDK degil, mevcut SDK'nin altinda/yaninda degerli olacak platform helper sinyalini veriyor."
        ]);
}

static class SpikeHelpers
{
    public static Process StartPythonAgent(string scriptPath)
    {
        var start = BuildPythonStartInfo(scriptPath);
        start.RedirectStandardInput = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.UseShellExecute = false;
        start.CreateNoWindow = true;

        var process = new Process { StartInfo = start };
        if (!process.Start())
        {
            throw new InvalidOperationException("Mock ACP agent could not be started.");
        }

        return process;
    }

    public static string FindMockAgent()
    {
        var current = AppContext.BaseDirectory;
        for (var i = 0; i < 8; i++)
        {
            var candidate = Path.Combine(current, "mock_acp_agent.py");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            var parent = Directory.GetParent(current);
            if (parent == null)
            {
                break;
            }

            current = parent.FullName;
        }

        var sourceCandidate = Path.Combine(Directory.GetCurrentDirectory(), "mock_acp_agent.py");
        if (File.Exists(sourceCandidate))
        {
            return sourceCandidate;
        }

        throw new FileNotFoundException("mock_acp_agent.py was not found.");
    }

    public static async Task StopProcessAsync(Process process, Transcript transcript, TimeSpan grace)
    {
        if (process.HasExited)
        {
            transcript.Event("process-exited", new { process.ExitCode });
            return;
        }

        try
        {
            process.StandardInput.Close();
            using var cts = new CancellationTokenSource(grace);
            await process.WaitForExitAsync(cts.Token);
            transcript.Event("process-graceful-exit", new { process.ExitCode });
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            transcript.Event("process-hard-kill", new { process.ExitCode });
        }
    }

    public static async Task DrainAsync(TextReader reader, Action<string> onLine)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            onLine(line);
        }
    }

    static ProcessStartInfo BuildPythonStartInfo(string scriptPath)
    {
        if (OperatingSystem.IsWindows())
        {
            var linuxPath = ToWslPath(scriptPath);
            return new ProcessStartInfo
            {
                FileName = "wsl.exe",
                Arguments = $"python3 {Quote(linuxPath)}"
            };
        }

        return new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = Quote(scriptPath)
        };
    }

    static string ToWslPath(string path)
    {
        if (path.StartsWith(@"\\wsl.localhost\", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            var relative = string.Join("/", parts.Skip(2));
            return "/" + relative;
        }

        return path.Replace('\\', '/');
    }

    static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";
}

sealed class ProbeClient(Transcript transcript) : IAcpClient
{
    public int StreamChunkCount { get; private set; }

    public ValueTask SessionNotificationAsync(SessionNotification notification, CancellationToken cancellationToken = default)
    {
        StreamChunkCount++;
        transcript.Event("session/update", notification);
        return ValueTask.CompletedTask;
    }

    public ValueTask<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("permission flow is out of scope for this spike");

    public ValueTask<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
        => new(new WriteTextFileResponse());

    public ValueTask<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request, CancellationToken cancellationToken = default)
        => new(new ReadTextFileResponse { Content = "mock content" });

    public ValueTask<CreateTerminalResponse> CreateTerminalAsync(CreateTerminalRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("terminal flow is out of scope for this spike");

    public ValueTask<TerminalOutputRequest> TerminalOutputAsync(TerminalOutputRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("terminal flow is out of scope for this spike");

    public ValueTask<ReleaseTerminalResponse> ReleaseTerminalAsync(ReleaseTerminalRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("terminal flow is out of scope for this spike");

    public ValueTask<WaitForTerminalExitResponse> WaitForTerminalExitAsync(WaitForTerminalExitRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("terminal flow is out of scope for this spike");

    public ValueTask<KillTerminalCommandResponse> KillTerminalCommandAsync(KillTerminalCommandRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("terminal flow is out of scope for this spike");

    public ValueTask<JsonElement> ExtMethodAsync(string method, JsonElement request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException($"extension method is out of scope: {method}");

    public ValueTask ExtNotificationAsync(string method, JsonElement notification, CancellationToken cancellationToken = default)
        => throw new NotSupportedException($"extension notification is out of scope: {method}");
}

sealed class StdioProcessBridge : IAsyncDisposable
{
    readonly Process process;
    readonly Dictionary<int, TaskCompletionSource<JsonElement>> pending = new();
    readonly CancellationTokenSource readCts = new();
    int nextId;

    public StdioProcessBridge(string scriptPath)
    {
        UsedWslBridge = OperatingSystem.IsWindows();
        Transcript = new Transcript();
        process = SpikeHelpers.StartPythonAgent(scriptPath);
        _ = SpikeHelpers.DrainAsync(process.StandardError, line => Transcript.Error(line));
        _ = ReadLoopAsync();
    }

    public Transcript Transcript { get; }
    public bool UsedWslBridge { get; }
    public int NotificationCount { get; private set; }

    public async Task<JsonElement> RequestAsync(string method, object parameters, CancellationToken cancellationToken = default)
    {
        var id = Interlocked.Increment(ref nextId);
        var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        pending[id] = tcs;

        await SendAsync(new { jsonrpc = "2.0", id, method, @params = parameters }, cancellationToken);

        await using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        return await tcs.Task;
    }

    public Task NotificationAsync(string method, object parameters, CancellationToken cancellationToken = default)
    {
        return SendAsync(new { jsonrpc = "2.0", method, @params = parameters }, cancellationToken);
    }

    public async Task StopAsync(TimeSpan grace)
    {
        readCts.Cancel();
        await SpikeHelpers.StopProcessAsync(process, Transcript, grace);
    }

    async Task SendAsync(object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);
        Transcript.Out(json);
        await process.StandardInput.WriteLineAsync(json.AsMemory(), cancellationToken);
        await process.StandardInput.FlushAsync(cancellationToken);
    }

    async Task ReadLoopAsync()
    {
        while (!readCts.IsCancellationRequested && await process.StandardOutput.ReadLineAsync() is { } line)
        {
            Transcript.In(line);
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement.Clone();

            if (root.TryGetProperty("id", out var idProperty) && idProperty.TryGetInt32(out var id) && pending.Remove(id, out var tcs))
            {
                if (root.TryGetProperty("error", out var error))
                {
                    tcs.TrySetException(new InvalidOperationException(error.ToString()));
                    continue;
                }

                tcs.TrySetResult(root.GetProperty("result").Clone());
                continue;
            }

            if (root.TryGetProperty("method", out _))
            {
                NotificationCount++;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        readCts.Cancel();
        if (!process.HasExited)
        {
            await StopAsync(TimeSpan.FromMilliseconds(200));
        }

        process.Dispose();
        readCts.Dispose();
    }
}

sealed class TranscriptTextWriter(TextWriter inner, Transcript transcript) : TextWriter
{
    public override Encoding Encoding => inner.Encoding;

    public override void WriteLine(string? value)
    {
        if (value != null)
        {
            transcript.Out(value);
        }

        inner.WriteLine(value);
        inner.Flush();
    }

    public override async Task WriteLineAsync(string? value)
    {
        if (value != null)
        {
            transcript.Out(value);
        }

        await inner.WriteLineAsync(value);
        await inner.FlushAsync();
    }
}

sealed class Transcript
{
    readonly List<string> entries = [];

    public int Count => entries.Count;

    public void Out(string json) => entries.Add(JsonSerializer.Serialize(new { direction = "out", json = JsonDocument.Parse(json).RootElement }));
    public void In(string json) => entries.Add(JsonSerializer.Serialize(new { direction = "in", json = JsonDocument.Parse(json).RootElement }));
    public void Error(string line) => entries.Add(JsonSerializer.Serialize(new { direction = "stderr", line }));
    public void Event(string name, object value) => entries.Add(JsonSerializer.Serialize(new { direction = "event", name, value }));

    public void Save(string fileName)
    {
        File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), fileName), entries);
    }
}

sealed record ProbeResult(
    string Name,
    int ProtocolScore,
    int StreamingScore,
    int LifecycleScore,
    int InteropScore,
    int DebugScore,
    string[] Notes);
