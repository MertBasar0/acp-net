using System.Text.Json;
using AcpNet.Process;
using AcpNet.Testing;
using AgentClientProtocol;

var artifactDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts");
var agentPath = FakeAcpAgentScript.WriteDefault(artifactDir);
var transcriptPath = Path.Combine(artifactDir, "sample-transcript.ndjson");

var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = [agentPath],
    Runtime = AcpRuntime.Auto,
    TranscriptPath = transcriptPath,
    RequiredExecutables = ["python3"],
    Shutdown = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromMilliseconds(500))
});

await using var session = await runner.StartAsync();
using var connection = new ClientSideConnection(_ => new SampleClient(), session.Stdout, session.Stdin);
connection.Open();

var init = await connection.InitializeAsync(new InitializeRequest
{
    ProtocolVersion = 1,
    ClientCapabilities = new ClientCapabilities()
});

var newSession = await connection.NewSessionAsync(new NewSessionRequest
{
    Cwd = session.ToAgentPath(Directory.GetCurrentDirectory()),
    McpServers = []
});

var prompt = await connection.PromptAsync(new PromptRequest
{
    SessionId = newSession.SessionId,
    Prompt = [new TextContentBlock { Text = "Hello from AcpProcessRunner" }]
});

await connection.CancelAsync(new CancelNotification { SessionId = newSession.SessionId });
await session.StopAsync();

Console.WriteLine($"protocol={init.ProtocolVersion}");
Console.WriteLine($"session={newSession.SessionId}");
Console.WriteLine($"stopReason={prompt.StopReason}");
Console.WriteLine($"usesWsl={session.UsesWsl}");
Console.WriteLine($"transcript={transcriptPath}");

sealed class SampleClient : IAcpClient
{
    public ValueTask SessionNotificationAsync(SessionNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification.Update is AgentMessageChunkSessionUpdate chunk && chunk.Content is TextContentBlock text)
        {
            Console.Write(text.Text);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
        => new(new WriteTextFileResponse());

    public ValueTask<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request, CancellationToken cancellationToken = default)
        => new(new ReadTextFileResponse { Content = "sample content" });

    public ValueTask<CreateTerminalResponse> CreateTerminalAsync(CreateTerminalRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<TerminalOutputRequest> TerminalOutputAsync(TerminalOutputRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<ReleaseTerminalResponse> ReleaseTerminalAsync(ReleaseTerminalRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<WaitForTerminalExitResponse> WaitForTerminalExitAsync(WaitForTerminalExitRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<KillTerminalCommandResponse> KillTerminalCommandAsync(KillTerminalCommandRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<JsonElement> ExtMethodAsync(string method, JsonElement request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(method);

    public ValueTask ExtNotificationAsync(string method, JsonElement notification, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(method);
}
