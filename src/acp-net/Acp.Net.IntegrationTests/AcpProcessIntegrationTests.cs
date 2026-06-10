using System.Text.Json;
using AcpNet.Process;
using AcpNet.Testing;
using AgentClientProtocol;
using Xunit;

namespace Acp.Net.IntegrationTests;

public sealed class AcpProcessIntegrationTests
{
    [Fact]
    public async Task AgentClientProtocolFlow_StreamsAndWritesTranscript()
    {
        var artifactDir = PrepareArtifactDir("flow");
        var agentPath = FakeAcpAgentScript.WriteDefault(artifactDir);
        var transcriptPath = Path.Combine(artifactDir, "flow-transcript.ndjson");
        var runArtifactPath = Path.Combine(artifactDir, "flow-run.json");

        var runner = new AcpProcessRunner(new AcpProcessOptions
        {
            AgentName = "fake-acp-agent",
            Command = "python3",
            Arguments = [agentPath],
            Runtime = AcpRuntime.Auto,
            TranscriptPath = transcriptPath,
            RunArtifactPath = runArtifactPath,
            RequiredTools = [AcpRequiredExecutable.Throw("python3")],
            Shutdown = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromMilliseconds(500))
        });

        await using var session = await runner.StartAsync();
        var client = new TestClient();
        using var connection = new ClientSideConnection(_ => client, session.Stdout, session.Stdin);
        connection.Open();

        var init = await connection.InitializeAsync(new InitializeRequest
        {
            ProtocolVersion = 1,
            ClientCapabilities = new ClientCapabilities()
        });

        Assert.Equal(1, init.ProtocolVersion);

        var agentCwd = session.ToAgentPath(Directory.GetCurrentDirectory());
        if (OperatingSystem.IsWindows())
        {
            Assert.StartsWith("/", agentCwd);
        }

        var newSession = await connection.NewSessionAsync(new NewSessionRequest
        {
            Cwd = agentCwd,
            McpServers = []
        });

        Assert.Equal("fake-session-1", newSession.SessionId);

        var prompt = await connection.PromptAsync(new PromptRequest
        {
            SessionId = newSession.SessionId,
            Prompt = [new TextContentBlock { Text = "hello" }]
        });

        Assert.Equal(StopReason.EndTurn, prompt.StopReason);
        Assert.Equal(["hello", " world"], client.StreamChunks);

        await connection.CancelAsync(new CancelNotification { SessionId = newSession.SessionId });
        await session.StopAsync();

        if (OperatingSystem.IsWindows())
        {
            Assert.True(session.UsesWsl);
        }

        AcpTranscriptAssert.ExistsAndNotEmpty(transcriptPath);
        AcpTranscriptAssert.Contains(transcriptPath, "session/prompt");
        AcpTranscriptAssert.Contains(transcriptPath, "session/update");
        AcpTranscriptAssert.Contains(transcriptPath, "preflight.tool.found");
        AcpTranscriptAssert.Contains(transcriptPath, "process.graceful_exit");

        Assert.True(File.Exists(runArtifactPath));
        var artifact = File.ReadAllText(runArtifactPath);
        Assert.Contains("\"agentName\": \"fake-acp-agent\"", artifact);
        Assert.Contains("\"result\": \"completed\"", artifact);
        Assert.Contains("\"failureKind\": \"None\"", artifact);
        Assert.Contains("\"name\": \"python3\"", artifact);
    }

    [Fact]
    public async Task StartAsync_ThrowsAndWritesArtifactWhenRequiredToolIsMissing()
    {
        var artifactDir = PrepareArtifactDir("preflight-fail");
        var agentPath = FakeAcpAgentScript.WriteDefault(artifactDir);
        var transcriptPath = Path.Combine(artifactDir, "preflight-fail-transcript.ndjson");
        var runArtifactPath = Path.Combine(artifactDir, "preflight-fail-run.json");

        var runner = new AcpProcessRunner(new AcpProcessOptions
        {
            AgentName = "fake-acp-agent",
            Command = "python3",
            Arguments = [agentPath],
            Runtime = AcpRuntime.Native,
            TranscriptPath = transcriptPath,
            RunArtifactPath = runArtifactPath,
            RequiredTools = [AcpRequiredExecutable.Throw("definitely-not-a-real-acp-required-tool")]
        });

        var exception = await Assert.ThrowsAsync<AcpPreflightException>(() => runner.StartAsync());

        Assert.Equal(AcpRunFailureKind.EnvironmentFailure, exception.FailureKind);
        Assert.Single(exception.Results);
        Assert.True(exception.Results[0].IsFailure);
        AcpTranscriptAssert.ExistsAndNotEmpty(transcriptPath);
        AcpTranscriptAssert.Contains(transcriptPath, "preflight.failed");

        Assert.True(File.Exists(runArtifactPath));
        var artifact = File.ReadAllText(runArtifactPath);
        Assert.Contains("\"result\": \"failed\"", artifact);
        Assert.Contains("\"failureKind\": \"EnvironmentFailure\"", artifact);
        Assert.Contains("\"missingPolicy\": \"Throw\"", artifact);
    }

    [Fact]
    public async Task StopAsync_HardKillsUnresponsiveAgent()
    {
        var artifactDir = PrepareArtifactDir("hard-kill");
        var agentPath = FakeAcpAgentScript.WriteHanging(artifactDir);
        var transcriptPath = Path.Combine(artifactDir, "hard-kill-transcript.ndjson");

        var runner = new AcpProcessRunner(new AcpProcessOptions
        {
            Command = "python3",
            Arguments = [agentPath],
            Runtime = AcpRuntime.Auto,
            TranscriptPath = transcriptPath,
            Shutdown = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromMilliseconds(100))
        });

        await using var session = await runner.StartAsync();
        await session.StopAsync();

        AcpTranscriptAssert.ExistsAndNotEmpty(transcriptPath);
        AcpTranscriptAssert.Contains(transcriptPath, "process.hard_kill");
    }

    static string PrepareArtifactDir(string name)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", name);
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, recursive: true);
        }

        Directory.CreateDirectory(dir);
        return dir;
    }

    sealed class TestClient : IAcpClient
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
            => throw new NotSupportedException();

        public ValueTask<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
            => new(new WriteTextFileResponse());

        public ValueTask<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request, CancellationToken cancellationToken = default)
            => new(new ReadTextFileResponse { Content = "test content" });

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
}
