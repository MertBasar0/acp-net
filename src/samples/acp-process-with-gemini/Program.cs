using System.Text.Json;
using AcpNet.Process;
using AgentClientProtocol;

var geminiPath = Environment.GetEnvironmentVariable("ACP_GEMINI_PATH");
if (string.IsNullOrWhiteSpace(geminiPath))
{
    geminiPath = "/home/mertb/.nvm/versions/node/v22.22.2/bin/gemini";
}

var promptText = args.Length > 0
    ? string.Join(" ", args)
    : "Reply with exactly this token and no extra commentary: ACP-DOGFOOD-OK";

var artifactDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts");
Directory.CreateDirectory(artifactDir);
var transcriptPath = Path.Combine(artifactDir, "gemini-dogfood-transcript.ndjson");

var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = geminiPath,
    Arguments = ["--acp", "--skip-trust", "--approval-mode", "plan"],
    WorkingDirectory = Directory.GetCurrentDirectory(),
    Runtime = AcpRuntime.Wsl,
    TranscriptPath = transcriptPath,
    RequiredExecutables = ["rg", "git", "node"],
    Shutdown = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromSeconds(2))
});

await using var session = await runner.StartAsync();
var client = new DogfoodClient();
using var connection = new ClientSideConnection(_ => client, session.Stdout, session.Stdin);
connection.Open();

using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(120));

try
{
    var init = await connection.InitializeAsync(new InitializeRequest
    {
        ProtocolVersion = 1,
        ClientCapabilities = new ClientCapabilities()
    }, timeout.Token);

    Console.WriteLine($"protocol={init.ProtocolVersion}");
    Console.WriteLine($"usesWsl={session.UsesWsl}");

    var agentCwd = session.ToAgentPath(Directory.GetCurrentDirectory());
    var newSession = await connection.NewSessionAsync(new NewSessionRequest
    {
        Cwd = agentCwd,
        McpServers = []
    }, timeout.Token);

    Console.WriteLine($"session={newSession.SessionId}");
    Console.WriteLine($"agentCwd={agentCwd}");

    var prompt = await connection.PromptAsync(new PromptRequest
    {
        SessionId = newSession.SessionId,
        Prompt = [new TextContentBlock { Text = promptText }]
    }, timeout.Token);

    Console.WriteLine();
    Console.WriteLine($"stopReason={prompt.StopReason}");
    Console.WriteLine($"chunks={client.StreamChunks.Count}");
    Console.WriteLine($"transcript={transcriptPath}");
}
finally
{
    await session.StopAsync(CancellationToken.None);
}

sealed class DogfoodClient : IAcpClient
{
    public List<string> StreamChunks { get; } = [];

    public ValueTask SessionNotificationAsync(SessionNotification notification, CancellationToken cancellationToken = default)
    {
        switch (notification.Update)
        {
            case AgentMessageChunkSessionUpdate chunk when chunk.Content is TextContentBlock text:
                StreamChunks.Add(text.Text);
                Console.Write(text.Text);
                break;
            case ToolCallSessionUpdate tool:
                Console.WriteLine();
                Console.WriteLine($"[tool] {tool.Title} ({tool.Status})");
                break;
            case ToolCallUpdateSessionUpdate toolUpdate:
                Console.WriteLine();
                Console.WriteLine($"[tool-update] {toolUpdate.ToolCallId} ({toolUpdate.Status})");
                break;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Gemini dogfood sample does not grant tool permissions.");
    }

    public ValueTask<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Gemini dogfood sample is read-only.");

    public ValueTask<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request, CancellationToken cancellationToken = default)
        => new(new ReadTextFileResponse { Content = "" });

    public ValueTask<CreateTerminalResponse> CreateTerminalAsync(CreateTerminalRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled for dogfood sample.");

    public ValueTask<TerminalOutputRequest> TerminalOutputAsync(TerminalOutputRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled for dogfood sample.");

    public ValueTask<ReleaseTerminalResponse> ReleaseTerminalAsync(ReleaseTerminalRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled for dogfood sample.");

    public ValueTask<WaitForTerminalExitResponse> WaitForTerminalExitAsync(WaitForTerminalExitRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled for dogfood sample.");

    public ValueTask<KillTerminalCommandResponse> KillTerminalCommandAsync(KillTerminalCommandRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Terminal access is disabled for dogfood sample.");

    public ValueTask<JsonElement> ExtMethodAsync(string method, JsonElement request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(method);

    public ValueTask ExtNotificationAsync(string method, JsonElement notification, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(method);
}
