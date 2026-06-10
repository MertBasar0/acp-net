# Acp.Net.Process

.NET ACP entegrasyonları için process, runtime köprüsü, kapatma ve transcript yardımcıları.

🇬🇧 English version: [README.md](README.md)

## Minimal Runtime Politikası Örneği

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

Eksik `Throw` araçları agent başlamadan önce başarısız olur ve `EnvironmentFailure` run artifact'i üretir. Eksik `Warn` araçları transcript'e ve artifact'e yazılır ama agent yine de başlar.

Bu paket bilinçli olarak ACP protokol şemasını modellemez. `AgentClientProtocol` gibi bir protokol paketiyle birlikte kullanın.

## Kurulum

```bash
dotnet add package Acp.Net.Process --prerelease
```

## Temel Kullanım

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

## Windows + WSL

Bir Windows .NET process'inin WSL/Linux ACP agent'ı çalıştırması gerektiğinde `AcpRuntime.Wsl` kullanın veya WSL path'leriyle `AcpRuntime.Auto`'yu bırakın:

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/user/agent.py"],
    Runtime = AcpRuntime.Wsl,
    WslDistribution = "Ubuntu"
});
```

Runner, UNC/Windows path'lerini WSL path'lerine eşler ve process'i `wsl.exe` üzerinden başlatır.
Agent WSL içinde çalışırken `NewSessionRequest.Cwd` gibi ACP payload path'leri için `session.ToAgentPath(...)` kullanın.

## Environment Şekillendirme

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

`RequiredExecutables`, agent başlamadan önce kontrol edilir ve transcript'e `preflight.tool.found` veya `preflight.tool.missing` olayları olarak yazılır.

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
