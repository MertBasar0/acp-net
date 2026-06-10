using System.Text.Json;
using AcpNet.Process;
using AcpNet.Testing;
using AgentClientProtocol;

var artifactDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss"));
Directory.CreateDirectory(artifactDir);

var agentPath = FakeAcpAgentScript.WriteDefault(artifactDir);
var transcriptPath = Path.Combine(artifactDir, "subagent-transcript.ndjson");
var runArtifactPath = Path.Combine(artifactDir, "subagent-run.json");

var runner = new AcpProcessRunner(new AcpProcessOptions
{
    AgentName = "openclaw-fake-acp-subagent",
    Command = "python3",
    Arguments = [agentPath],
    WorkingDirectory = Directory.GetCurrentDirectory(),
    Runtime = AcpRuntime.Auto,
    TranscriptPath = transcriptPath,
    RunArtifactPath = runArtifactPath,
    RequiredTools =
    [
        AcpRequiredExecutable.Throw("python3"),
        AcpRequiredExecutable.Warn("rg")
    ],
    Shutdown = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromMilliseconds(500))
});

await using var session = await runner.StartAsync();
var client = new OpenClawStyleClient();
using var connection = new ClientSideConnection(_ => client, session.Stdout, session.Stdin);
connection.Open();

var init = await connection.InitializeAsync(new InitializeRequest
{
    ProtocolVersion = 1,
    ClientCapabilities = new ClientCapabilities()
});

var agentCwd = session.ToAgentPath(Directory.GetCurrentDirectory());
var newSession = await connection.NewSessionAsync(new NewSessionRequest
{
    Cwd = agentCwd,
    McpServers = []
});

var prompt = await connection.PromptAsync(new PromptRequest
{
    SessionId = newSession.SessionId,
    Prompt = [new TextContentBlock { Text = "Summarize this OpenClaw delegated task result." }]
});

await session.StopAsync();

var openClawResult = new
{
    kind = "openclaw.subagent.result",
    protocol = init.ProtocolVersion,
    sessionId = newSession.SessionId,
    stopReason = prompt.StopReason.ToString(),
    chunks = client.StreamChunks,
    transcriptPath,
    runArtifactPath
};

Console.WriteLine(JsonSerializer.Serialize(openClawResult, new JsonSerializerOptions { WriteIndented = true }));

sealed class OpenClawStyleClient : IAcpClient
{
    public List<string> StreamChunks { get; } = [];

    public ValueTask SessionNotificationAsync(SessionNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Update is AgentMessageChunkSessionUpdate chunk && chunk.Content is TextContentBlock text)
        {
            StreamChunks.Add(text.Text);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("OpenClaw sample does not grant permissions.");

    public ValueTask<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("OpenClaw sample is read-only.");

    public ValueTask<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request, CancellationToken cancellationToken = default)
        => new(new ReadTextFileResponse { Content = "" });

    public ValueTask<CreateTerminalResponse> CreateTerminalAsync(CreateTerminalRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled.");

    public ValueTask<TerminalOutputRequest> TerminalOutputAsync(TerminalOutputRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled.");

    public ValueTask<ReleaseTerminalResponse> ReleaseTerminalAsync(ReleaseTerminalRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled.");

    public ValueTask<WaitForTerminalExitResponse> WaitForTerminalExitAsync(WaitForTerminalExitRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled.");

    public ValueTask<KillTerminalCommandResponse> KillTerminalCommandAsync(KillTerminalCommandRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled.");

    public ValueTask<JsonElement> ExtMethodAsync(string method, JsonElement request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(method);

    public ValueTask ExtNotificationAsync(string method, JsonElement notification, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(method);
}
