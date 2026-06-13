# Acp.Net.Process

Process, runtime bridge, shutdown, and transcript helpers for .NET ACP integrations.

🇹🇷 Türkçe sürüm: [README.tr.md](README.tr.md)

## Minimal Runtime Policy Example

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    AgentName = "my-acp-agent",
    Command = "python3",
    Arguments = ["agent.py"],
    TranscriptPath = "artifacts/agent-transcript.ndjson",
    RunArtifactPath = "artifacts/agent-run.json",
    RequiredTools =
    [
        AcpRequiredExecutable.Throw("python3"),
        AcpRequiredExecutable.Warn("rg")
    ]
});
```

Missing `Throw` tools fail before the agent starts and produce an `EnvironmentFailure` run artifact. Missing `Warn` tools are written to the transcript and artifact but the agent still starts.

This package intentionally does not model the ACP protocol schema. Use it with a protocol package such as `AgentClientProtocol`.

## Install

```bash
dotnet add package Acp.Net.Process --prerelease
```

## Basic Usage

```csharp
using AcpNet.Process;
using AgentClientProtocol;

var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/user/agent.py"],
    Runtime = AcpRuntime.Auto,
    TranscriptPath = "agent-transcript.ndjson",
    RequiredExecutables = ["rg", "git"],
    Shutdown = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromSeconds(2))
});

await using var session = await runner.StartAsync();

using var connection = new ClientSideConnection(
    _ => client,
    session.Stdout,
    session.Stdin);

connection.Open();

var cwdForAgent = session.ToAgentPath(Directory.GetCurrentDirectory());
```

> **Windows note:** `AcpRuntime.Auto` runs the command natively unless it detects a
> WSL agent. For a POSIX agent such as `python3`, set `Runtime = AcpRuntime.Wsl`
> explicitly (see below), or keep `Auto` with a WSL agent path. As of `0.1.0-alpha.2`,
> when a bare command like `python3` resolves only to a Windows Store execution-alias
> stub, `Auto` routes it through WSL and preflight reports the stub as missing instead
> of hanging.

## Windows + WSL

When a Windows .NET process needs to run a WSL/Linux ACP agent, use `AcpRuntime.Wsl` or leave `AcpRuntime.Auto` with WSL paths:

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/user/agent.py"],
    Runtime = AcpRuntime.Wsl,
    WslDistribution = "Ubuntu"
});
```

The runner maps UNC/Windows paths to WSL paths and starts the process through `wsl.exe`.
Use `session.ToAgentPath(...)` for ACP payload paths such as `NewSessionRequest.Cwd` when the agent runs in WSL.

## Environment Shaping

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "gemini",
    Arguments = ["--acp"],
    Runtime = AcpRuntime.Wsl,
    AdditionalPathEntries = ["/usr/bin", "/home/user/.local/bin"],
    Environment = new Dictionary<string, string?>
    {
        ["GEMINI_DEBUG"] = "1"
    },
    RequiredExecutables = ["rg", "git", "node"]
});
```

`RequiredExecutables` are checked before the agent starts and written to the transcript as `preflight.tool.found` or `preflight.tool.missing` events.

## Public API

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpProcessSession`
- `AcpRequiredExecutable`
- `AcpMissingExecutablePolicy`
- `AcpExecutablePreflightResult`
- `AcpPreflightException`
- `AcpRunArtifact`
- `AcpRunFailureKind`
- `AcpRuntime`
- `AcpShutdownPolicy`
- `AcpTranscriptRecorder`
