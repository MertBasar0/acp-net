using System.Text.Json;
using System.Text.Json.Serialization;
using AcpNet.Process;
using AcpNet.Testing;
using AgentClientProtocol;

var jsonOptions = CreateJsonOptions();
var options = ProbeOptions.Parse(args);

if (options.ShowHelp)
{
    Console.Error.WriteLine(ProbeOptions.HelpText);
    return 0;
}

if (options.ConfigError is { } configError)
{
    WriteJson(new
    {
        kind = "openclaw.acpnet.probe.result",
        ok = false,
        result = "failed",
        failureKind = "ConfigurationFailure",
        failureMessage = configError
    });
    return 64;
}

var artifactDir = options.ArtifactDirectory
    ?? Path.Combine(options.WorkingDirectory, "artifacts", "openclaw-probe", DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss"));
Directory.CreateDirectory(artifactDir);

var command = options.Command;
var commandArguments = options.Arguments.ToArray();
if (string.IsNullOrWhiteSpace(command))
{
    var agentPath = FakeAcpAgentScript.WriteDefault(artifactDir);
    command = "python3";
    commandArguments = [agentPath];
}

var transcriptPath = options.TranscriptPath ?? Path.Combine(artifactDir, "probe-transcript.ndjson");
var runArtifactPath = options.RunArtifactPath ?? Path.Combine(artifactDir, "probe-run.json");
var requiredTools = ResolveRequiredTools(options, command);

try
{
    using var timeout = new CancellationTokenSource(options.Timeout);
    var runner = new AcpProcessRunner(new AcpProcessOptions
    {
        AgentName = options.AgentName,
        Command = command,
        Arguments = commandArguments,
        WorkingDirectory = options.WorkingDirectory,
        Runtime = options.Runtime,
        WslDistribution = options.WslDistribution,
        TranscriptPath = transcriptPath,
        RunArtifactPath = runArtifactPath,
        RequiredExecutables = requiredTools,
        Shutdown = AcpShutdownPolicy.GracefulThenKill(options.ShutdownGracePeriod)
    });

    await using var session = await runner.StartAsync(timeout.Token);
    var client = new ProbeClient();
    using var connection = new ClientSideConnection(_ => client, session.Stdout, session.Stdin);
    connection.Open();

    var init = await connection.InitializeAsync(new InitializeRequest
    {
        ProtocolVersion = 1,
        ClientCapabilities = new ClientCapabilities()
    }, timeout.Token);

    var newSession = await connection.NewSessionAsync(new NewSessionRequest
    {
        Cwd = session.ToAgentPath(options.WorkingDirectory),
        McpServers = []
    }, timeout.Token);

    var prompt = await connection.PromptAsync(new PromptRequest
    {
        SessionId = newSession.SessionId,
        Prompt = [new TextContentBlock { Text = options.Prompt }]
    }, timeout.Token);

    await session.StopAsync(CancellationToken.None);

    var artifact = ReadArtifact(runArtifactPath);
    var preflight = artifact?.Preflight ?? [];
    var criticalMissing = preflight
        .Where(item => !item.Found && item.MissingPolicy == AcpMissingExecutablePolicy.Throw)
        .ToArray();
    var warnings = preflight
        .Where(item => !item.Found && item.MissingPolicy != AcpMissingExecutablePolicy.Throw)
        .ToArray();
    var ok = artifact?.Result == "completed"
        && artifact.FailureKind == AcpRunFailureKind.None
        && criticalMissing.Length == 0;

    WriteJson(new
    {
        kind = "openclaw.acpnet.probe.result",
        ok,
        result = artifact?.Result ?? "unknown",
        failureKind = artifact?.FailureKind.ToString() ?? AcpRunFailureKind.Unknown.ToString(),
        failureMessage = artifact?.FailureMessage,
        protocol = init.ProtocolVersion,
        sessionId = newSession.SessionId,
        stopReason = prompt.StopReason.ToString(),
        chunks = client.StreamChunks,
        agentName = artifact?.AgentName ?? options.AgentName,
        usesWsl = artifact?.UsesWsl,
        runArtifactPath,
        transcriptPath,
        preflight = BuildPreflightSummary(preflight, criticalMissing, warnings)
    });

    return ok ? 0 : 3;
}
catch (AcpPreflightException ex)
{
    WriteJson(new
    {
        kind = "openclaw.acpnet.probe.result",
        ok = false,
        result = "failed",
        failureKind = ex.FailureKind.ToString(),
        failureMessage = ex.Message,
        runArtifactPath,
        transcriptPath,
        preflight = BuildPreflightSummary(
            ex.Results,
            ex.Results.Where(item => item.IsFailure).ToArray(),
            ex.Results.Where(item => !item.Found && !item.IsFailure).ToArray())
    });
    return 2;
}
catch (OperationCanceledException)
{
    WriteJson(new
    {
        kind = "openclaw.acpnet.probe.result",
        ok = false,
        result = "failed",
        failureKind = AcpRunFailureKind.ProcessFailure.ToString(),
        failureMessage = $"Probe timed out after {options.Timeout.TotalSeconds:0.###} seconds.",
        runArtifactPath,
        transcriptPath
    });
    return 3;
}
catch (Exception ex)
{
    WriteJson(new
    {
        kind = "openclaw.acpnet.probe.result",
        ok = false,
        result = "failed",
        failureKind = AcpRunFailureKind.Unknown.ToString(),
        failureMessage = ex.Message,
        runArtifactPath,
        transcriptPath
    });
    return 3;
}

static IReadOnlyList<AcpRequiredExecutable> ResolveRequiredTools(ProbeOptions options, string command)
{
    var tools = new List<AcpRequiredExecutable>();
    tools.AddRange(options.RequiredTools.Select(AcpRequiredExecutable.Throw));
    tools.AddRange(options.OptionalTools.Select(AcpRequiredExecutable.Warn));

    if (!options.UsesCustomCommand && tools.Count == 0)
    {
        tools.Add(AcpRequiredExecutable.Throw("python3"));
        tools.Add(AcpRequiredExecutable.Warn("rg"));
    }

    if (options.RequireCommandExecutable && !Path.IsPathRooted(command))
    {
        tools.Insert(0, AcpRequiredExecutable.Throw(command));
    }

    return tools;
}

static AcpRunArtifact? ReadArtifact(string runArtifactPath)
{
    if (!File.Exists(runArtifactPath))
    {
        return null;
    }

    return JsonSerializer.Deserialize<AcpRunArtifact>(File.ReadAllText(runArtifactPath), CreateJsonOptions());
}

static object BuildPreflightSummary(
    IReadOnlyList<AcpExecutablePreflightResult> preflight,
    IReadOnlyList<AcpExecutablePreflightResult> criticalMissing,
    IReadOnlyList<AcpExecutablePreflightResult> warnings)
{
    return new
    {
        total = preflight.Count,
        criticalMissing = criticalMissing.Select(ToToolSummary).ToArray(),
        warnings = warnings.Select(ToToolSummary).ToArray(),
        tools = preflight.Select(ToToolSummary).ToArray()
    };
}

static object ToToolSummary(AcpExecutablePreflightResult item)
{
    return new
    {
        name = item.Name,
        found = item.Found,
        path = item.Path,
        error = item.Error,
        missingPolicy = item.MissingPolicy.ToString()
    };
}

static void WriteJson(object value)
{
    Console.WriteLine(JsonSerializer.Serialize(value, CreateJsonOptions()));
}

static JsonSerializerOptions CreateJsonOptions()
{
    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    options.Converters.Add(new JsonStringEnumConverter());
    return options;
}

sealed record ProbeOptions
{
    public string AgentName { get; init; } = "openclaw-fake-acp-subagent";

    public string WorkingDirectory { get; init; } = Directory.GetCurrentDirectory();

    public string? Command { get; init; }

    public IReadOnlyList<string> Arguments { get; init; } = [];

    public IReadOnlyList<string> RequiredTools { get; init; } = [];

    public IReadOnlyList<string> OptionalTools { get; init; } = [];

    public string? TranscriptPath { get; init; }

    public string? RunArtifactPath { get; init; }

    public string? ArtifactDirectory { get; init; }

    public string Prompt { get; init; } = "OpenClaw delegated probe task.";

    public AcpRuntime Runtime { get; init; } = AcpRuntime.Auto;

    public string? WslDistribution { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(120);

    public TimeSpan ShutdownGracePeriod { get; init; } = TimeSpan.FromMilliseconds(500);

    public bool RequireCommandExecutable { get; init; }

    public bool UsesCustomCommand => !string.IsNullOrWhiteSpace(Command);

    public bool ShowHelp { get; init; }

    public string? ConfigError { get; init; }

    public static string HelpText => """
Usage:
  openclaw-acpnet-probe [options]

Defaults:
  Without --command, the probe runs the deterministic fake ACP agent and spends no model quota.

Options:
  --agent <name>                 Agent name written to run artifact.
  --cwd <path>                   Working directory for the ACP run.
  --command <command>            ACP agent command. If omitted, fake ACP agent is used.
  --arg <value>                  Agent command argument. Repeatable.
  --required-tool <name>         Critical executable preflight. Repeatable.
  --optional-tool <name>         Warning-only executable preflight. Repeatable.
  --require-command-executable   Add --command itself as a critical preflight tool when it is not rooted.
  --transcript <path>            Transcript NDJSON output path.
  --artifact <path>              Run artifact JSON output path.
  --artifact-dir <path>          Directory for default transcript/artifact paths.
  --prompt <text>                Prompt text sent to the ACP agent.
  --runtime <auto|native|wsl>    Runtime mode.
  --wsl-distribution <name>      WSL distribution name when runtime is WSL.
  --timeout-seconds <seconds>    Overall ACP probe timeout. Default: 120.
  --shutdown-ms <ms>             Graceful shutdown period before kill. Default: 500.
  --help                         Print this help to stderr.

Exit codes:
  0   ok=true
  2   environment/preflight failure
  3   runtime/protocol/agent/unknown failure
  64  invalid CLI configuration
""";

    public static ProbeOptions Parse(IReadOnlyList<string> args)
    {
        var agentName = "openclaw-fake-acp-subagent";
        var cwd = Directory.GetCurrentDirectory();
        string? command = null;
        var commandArgs = new List<string>();
        var requiredTools = new List<string>();
        var optionalTools = new List<string>();
        string? transcript = null;
        string? artifact = null;
        string? artifactDir = null;
        var prompt = "OpenClaw delegated probe task.";
        var runtime = AcpRuntime.Auto;
        string? wslDistribution = null;
        var timeout = TimeSpan.FromSeconds(120);
        var shutdown = TimeSpan.FromMilliseconds(500);
        var requireCommandExecutable = false;

        for (var i = 0; i < args.Count; i++)
        {
            var current = args[i];
            switch (current)
            {
                case "--help":
                case "-h":
                    return new ProbeOptions { ShowHelp = true };
                case "--agent":
                    if (!TryReadValue(args, ref i, out agentName, out var agentError))
                    {
                        return Invalid(agentError);
                    }
                    break;
                case "--cwd":
                    if (!TryReadValue(args, ref i, out cwd, out var cwdError))
                    {
                        return Invalid(cwdError);
                    }
                    cwd = Path.GetFullPath(cwd);
                    break;
                case "--command":
                    if (!TryReadValue(args, ref i, out command, out var commandError))
                    {
                        return Invalid(commandError);
                    }
                    break;
                case "--arg":
                    // --arg forwards a single token to the agent command verbatim,
                    // so it must accept flag-style values like `--acp`. Using the
                    // normal value reader here would reject any `--`-prefixed agent
                    // flag and break real-agent invocations.
                    if (!TryReadRawValue(args, ref i, out var commandArg, out var argError))
                    {
                        return Invalid(argError);
                    }
                    commandArgs.Add(commandArg);
                    break;
                case "--required-tool":
                    if (!TryReadValue(args, ref i, out var requiredTool, out var requiredError))
                    {
                        return Invalid(requiredError);
                    }
                    requiredTools.Add(requiredTool);
                    break;
                case "--optional-tool":
                    if (!TryReadValue(args, ref i, out var optionalTool, out var optionalError))
                    {
                        return Invalid(optionalError);
                    }
                    optionalTools.Add(optionalTool);
                    break;
                case "--require-command-executable":
                    requireCommandExecutable = true;
                    break;
                case "--transcript":
                    if (!TryReadValue(args, ref i, out transcript, out var transcriptError))
                    {
                        return Invalid(transcriptError);
                    }
                    transcript = Path.GetFullPath(transcript);
                    break;
                case "--artifact":
                    if (!TryReadValue(args, ref i, out artifact, out var artifactError))
                    {
                        return Invalid(artifactError);
                    }
                    artifact = Path.GetFullPath(artifact);
                    break;
                case "--artifact-dir":
                    if (!TryReadValue(args, ref i, out artifactDir, out var artifactDirError))
                    {
                        return Invalid(artifactDirError);
                    }
                    artifactDir = Path.GetFullPath(artifactDir);
                    break;
                case "--prompt":
                    if (!TryReadValue(args, ref i, out prompt, out var promptError))
                    {
                        return Invalid(promptError);
                    }
                    break;
                case "--runtime":
                    if (!TryReadValue(args, ref i, out var runtimeValue, out var runtimeError))
                    {
                        return Invalid(runtimeError);
                    }
                    if (!TryParseRuntime(runtimeValue, out runtime))
                    {
                        return Invalid($"Invalid --runtime value: {runtimeValue}");
                    }
                    break;
                case "--wsl-distribution":
                    if (!TryReadValue(args, ref i, out wslDistribution, out var wslError))
                    {
                        return Invalid(wslError);
                    }
                    break;
                case "--timeout-seconds":
                    if (!TryReadValue(args, ref i, out var timeoutValue, out var timeoutError))
                    {
                        return Invalid(timeoutError);
                    }
                    if (!TryParsePositiveDouble(timeoutValue, out var timeoutSeconds))
                    {
                        return Invalid($"Invalid --timeout-seconds value: {timeoutValue}");
                    }
                    timeout = TimeSpan.FromSeconds(timeoutSeconds);
                    break;
                case "--shutdown-ms":
                    if (!TryReadValue(args, ref i, out var shutdownValue, out var shutdownError))
                    {
                        return Invalid(shutdownError);
                    }
                    if (!TryParsePositiveDouble(shutdownValue, out var shutdownMs))
                    {
                        return Invalid($"Invalid --shutdown-ms value: {shutdownValue}");
                    }
                    shutdown = TimeSpan.FromMilliseconds(shutdownMs);
                    break;
                default:
                    return Invalid($"Unknown argument: {current}");
            }
        }

        if (command is null && commandArgs.Count > 0)
        {
            return Invalid("--arg requires --command.");
        }

        return new ProbeOptions
        {
            AgentName = agentName,
            WorkingDirectory = Path.GetFullPath(cwd),
            Command = command,
            Arguments = commandArgs,
            RequiredTools = requiredTools,
            OptionalTools = optionalTools,
            TranscriptPath = transcript,
            RunArtifactPath = artifact,
            ArtifactDirectory = artifactDir,
            Prompt = prompt,
            Runtime = runtime,
            WslDistribution = wslDistribution,
            Timeout = timeout,
            ShutdownGracePeriod = shutdown,
            RequireCommandExecutable = requireCommandExecutable
        };
    }

    static ProbeOptions Invalid(string message) => new() { ConfigError = message };

    static bool TryReadValue(IReadOnlyList<string> args, ref int index, out string value, out string error)
    {
        if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            value = string.Empty;
            error = $"{args[index]} requires a value.";
            return false;
        }

        value = args[++index];
        error = string.Empty;
        return true;
    }

    // Reads the next token verbatim without rejecting `--`-prefixed values. Only
    // for options like --arg that forward a token to the agent command, where a
    // leading `--` is a normal agent flag rather than the next probe option.
    static bool TryReadRawValue(IReadOnlyList<string> args, ref int index, out string value, out string error)
    {
        if (index + 1 >= args.Count)
        {
            value = string.Empty;
            error = $"{args[index]} requires a value.";
            return false;
        }

        value = args[++index];
        error = string.Empty;
        return true;
    }

    static bool TryParseRuntime(string value, out AcpRuntime runtime)
    {
        runtime = value.Trim().ToLowerInvariant() switch
        {
            "auto" => AcpRuntime.Auto,
            "native" => AcpRuntime.Native,
            "wsl" => AcpRuntime.Wsl,
            _ => AcpRuntime.Auto
        };
        return value.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("native", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("wsl", StringComparison.OrdinalIgnoreCase);
    }

    static bool TryParsePositiveDouble(string value, out double parsed)
    {
        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsed)
            && parsed > 0;
    }
}

sealed class ProbeClient : IAcpClient
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
        => throw new InvalidOperationException("OpenClaw probe does not grant permissions.");

    public ValueTask<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("OpenClaw probe is read-only.");

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
