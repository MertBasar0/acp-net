# Acp.Net MVP Product Design

> 🇹🇷 Türkçe sürüm: [acp-net-mvp-product-design.tr.md](acp-net-mvp-product-design.tr.md)

Date: 2026-06-07

## Product Claim

Acp.Net will not be positioned as a new full ACP protocol SDK for .NET.

Acp.Net's initial product claim is:

> A process/runtime/testing layer for reliably starting, managing, debugging, and testing ACP agent processes from .NET applications.

This position aims to complement the existing `AgentClientProtocol` package instead of replacing it.

## Target User

Initial target users:

- A developer who wants to start an ACP agent from a .NET application.
- A developer running a WSL/Linux agent on Windows.
- An SDK/IDE/plugin developer who wants to test stdio-based agent integration.
- A team that wants deterministic agent process lifecycle tests in CI.

Out of scope:

- People who want to remodel the ACP protocol specification from scratch.
- People looking for UI/editor integration.
- Training Factory or RL productization.
- People looking for a general-purpose process manager.

## MVP Packages

The first packaging splits into two parts.

### Acp.Net.Process

Responsibilities:

- Starting the ACP agent process.
- Managing stdin/stdout/stderr.
- Windows/WSL runtime bridge.
- Path normalization.
- Timeout and shutdown strategy.
- Raw transcript capture.

This package does not own protocol schemas. It must be able to work together with the existing `AgentClientProtocol` package.

### Acp.Net.Testing

Responsibilities:

- Fake ACP agent/server.
- Transcript assertion helpers.
- Golden transcript tests.
- Test fixtures for timeout/cancel/process-exit scenarios.

This package must stay test-focused; production runtime dependencies must be kept minimal.

## In Scope For The MVP

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpRuntimeResolver`
- `AcpPathMapper`
- `AcpTranscriptRecorder`
- `AcpShutdownPolicy`
- `FakeAcpAgent`
- transcript assertion helpers

## Out Of Scope For The MVP

- Full ACP schema model.
- Typed protocol surface such as `InitializeRequest`, `PromptRequest`.
- UI/dashboard.
- Provider marketplace.
- Training Factory integration.
- PX4/Gazebo/drone workflow.
- Long-running daemon/service management.

## Initial API Sketch

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/<user>/agent.py"],
    WorkingDirectory = "/home/<user>/project",
    Runtime = AcpRuntime.Auto,
    TranscriptPath = "agent-transcript.ndjson",
    Shutdown = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromSeconds(2))
});

await using var session = await runner.StartAsync();

using var connection = new ClientSideConnection(
    _ => client,
    session.Stdout,
    session.Stdin);

connection.Open();

var init = await connection.InitializeAsync(...);
```

For a Windows host + WSL agent:

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/<user>/agent.py"],
    Runtime = AcpRuntime.Wsl,
    WslDistribution = "Ubuntu"
});
```

For the test helper:

```csharp
await using var fake = await FakeAcpAgent.StartAsync(new FakeAcpAgentOptions
{
    Script = FakeAcpScript.Default()
        .OnInitialize()
        .OnNewSession("test-session")
        .OnPrompt(stream: ["hello", " world"], stopReason: "end_turn")
});

var runner = AcpProcessRunner.ForExistingProcess(fake.Process);
```

## Success Criteria

For the MVP to count as valuable:

1. It offers a minimal sample working together with `AgentClientProtocol`.
2. It runs the Windows `dotnet.exe` -> WSL `python3` agent scenario with a single setting.
3. It records the stdin/stdout/stderr flow as a lossless transcript.
4. If the process does not exit, it applies graceful shutdown first, then hard kill.
5. Timeout/cancel/streaming tests run deterministically in CI with the fake ACP agent.
6. It visibly reduces the amount of process glue code in integrating applications.

## Product Value

The value is not in protocol typing but in lowering integration cost.

Where the existing package is good:

- ACP method names.
- Schema types.
- Request/notification dispatch.

Where Acp.Net must be good:

- Real process behavior.
- Windows/WSL reality.
- Debuggability.
- Testability.

## Risks

- Scope can drift back toward a full SDK.
- If the existing `AgentClientProtocol` package adds these helpers, the differentiation shrinks.
- Windows/WSL scenarios can vary by machine and distro.
- If test helpers and the production runner land in the same package, the API can bloat.

## Mitigation

- Do not write protocol schemas; prefer integration with the existing package.
- Keep Process and Testing packages separate.
- Focus the first MVP only on the stdio ACP agent scenario.
- Every feature should come from a failing integration test.

## Design Notes Log

The notes below record how the design evolved during implementation. The detailed spike session reports behind them are maintained as the maintainer's local engineering notes outside this repository.

### 2026-06-07 API Hardening Note

Package ids stayed `Acp.Net.Process` and `Acp.Net.Testing`. The C# namespaces were simplified to `AcpNet.Process` and `AcpNet.Testing`, because an `Acp.Net.Process` namespace creates an unnecessary name collision with `System.Diagnostics.Process`.

The first alpha public surface was narrowed to:

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpProcessSession`
- `AcpRuntime`
- `AcpShutdownPolicy`
- `AcpTranscriptRecorder`
- `FakeAcpAgentScript`
- `AcpTranscriptAssert`

### 2026-06-07 Gemini Dogfood Note

Dogfooding with the real Gemini CLI ACP agent succeeded. It exposed the need for `AcpProcessSession.ToAgentPath(...)`: when the agent runs inside WSL, not only the process start path but also path fields inside ACP payloads such as `cwd` must be converted to WSL paths.

Additional productization requirement:

- PATH differences can appear in WSL non-login process environments. Gemini stderr showed `Ripgrep is not available. Falling back to GrepTool.`. The runner should eventually offer an environment/PATH or login-shell strategy.

### 2026-06-07 Runtime Environment Shaping Note

The first APIs for this requirement were added:

- `AcpProcessOptions.Environment`
- `AcpProcessOptions.AdditionalPathEntries`
- `AcpProcessOptions.RequiredExecutables`

The runner now preflights required executables before the agent starts and writes `preflight.tool.found` or `preflight.tool.missing` events into the transcript.

In the Gemini dogfood, a missing `rg` was caught as `preflight.tool.missing` before the agent ran; `git` and `node` were found.

### 2026-06-09 OpenClaw Runtime Substrate Note

After spikes 008–010, Acp.Net's probable role inside OpenClaw became clearer.

Acp.Net is not a core orchestrator for OpenClaw; it should be positioned as a runtime substrate that runs ACP-compatible agent/subagent processes reliably and auditably.

Added product capabilities:

- per-tool preflight policy: `Warn` or `Throw`
- fail-fast environment failure
- `AcpPreflightException`
- `AcpRunFailureKind`
- machine-readable `AcpRunArtifact`
- `RunArtifactPath`
- an OpenClaw-style deterministic subagent runner sample

This decision makes the following distinction central to productization:

> Agent failure and environment failure are not the same thing.

An orchestrator like OpenClaw must know the health of the runtime environment before judging the agent result. Acp.Net's value is providing that evidence layer.

### 2026-06-09 Spike 011 OpenClaw Probe Note

The OpenClaw source tree already provides ACP runtime/backend and process lease management through `extensions/acpx`. Adding Acp.Net as a second runtime inside OpenClaw is therefore premature.

Two integration shapes were tried in spike 011:

- The C# external command probe succeeded.
- The Node wrapper probe hit a `UtilBindVsockAnyPort` error at the Node child process -> Windows interop boundary under a sandboxed environment.

This finding does not reduce Acp.Net's value; it shows again how critical the runtime boundary is for OpenClaw integration.

### 2026-06-09 Spike 012 ACPX Contract Decision

The OpenClaw `extensions/acpx` and `packages/acp-core` contracts were reviewed.

Decision:

> Acp.Net must not be an ACPX runtime backend replacement.

Rationale:

- ACPX already implements OpenClaw's session/turn/event runtime contract.
- ACPX integrates process lease and cleanup state with OpenClaw's plugin state system.
- Acp.Net's strongest side is not runtime event streaming; it is process evidence, preflight, failure classification, and the test harness.

Short-term product path:

1. Stabilize the Acp.Net diagnostic command contract.
2. Let OpenClaw call it as a doctor/plugin command.
3. Demonstrate the environment-failure vs agent-failure distinction through artifacts and transcripts.
4. Enter deeper ACPX integration only after the command contract proves useful.

### 2026-06-09 Spike 013 Diagnostic Command Note

`openclaw-acpnet-probe` now provides a more stable CLI contract.

Supported decisions:

- stdout is reserved for one JSON result.
- The exit code contract is fixed: `0`, `2`, `3`, `64`.
- The fake-agent default is preserved, giving a verification path that spends no model quota.
- `--command` and repeatable `--arg` were added for real ACP-compatible commands.
- Tool policy arguments became external: `--required-tool` and `--optional-tool`.

This is the necessary intermediate productization step before touching OpenClaw core.

### 2026-06-09 Spike 014 Doctor Adapter Note

The mapping contract from the diagnostic command result to OpenClaw doctor/lint surfaces was prepared.

Decision:

- `AcpRuntimeDoctorReport` is the right surface for the runtime doctor.
- `HealthFinding[]` is the right surface for doctor lint.
- A missing optional tool can stay `ok=true` for the runtime doctor but must be a `warning` finding on the lint surface.
- A critical environment failure must be an error for both doctor and lint.

This reinforces that Acp.Net's first OpenClaw integration should take the shape of doctor/lint evidence.
