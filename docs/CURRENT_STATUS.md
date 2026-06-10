# Current Status

Last updated: 2026-06-10

## Product Position

Acp.Net is an independent .NET package family for ACP-compatible agent process runtime, diagnostics, and testing.

The current decision is:

> Build Acp.Net as a standalone package family. Do not make OpenClaw core integration the product goal.

OpenClaw is useful as:

- a real reference consumer,
- a dogfood environment,
- a source of concrete runtime requirements,
- a future sample integration.

OpenClaw core changes should be treated as a later proposal, not the default development path.

## Implemented Code

Solution:

`src/acp-net/AcpNetMvp.slnx`

Projects:

- `Acp.Net.Process`
- `Acp.Net.Testing`
- `Acp.Net.UnitTests`
- `Acp.Net.IntegrationTests`

Samples/tools:

- `src/samples/acp-process-with-agentclientprotocol/`
- `src/samples/acp-process-with-gemini/`
- `src/samples/openclaw-subagent-runner/`
- `src/samples/openclaw-acpnet-probe/`
- `src/openclaw-probe/`

## Implemented Capabilities

- process launch through `AcpProcessRunner`
- native/WSL runtime resolution
- Windows/WSL path mapping
- `AcpProcessSession.ToAgentPath(...)`
- environment variable shaping
- `AdditionalPathEntries`
- required/optional executable preflight
- warning vs fail-fast preflight policy
- `AcpPreflightException`
- run failure classification
- transcript recording
- run artifact JSON
- graceful shutdown then hard kill
- deterministic fake ACP agent
- hanging fake agent fixture
- transcript assertions
- OpenClaw-style diagnostic probe
- OpenClaw doctor/lint mapping draft

## Latest Verification

Last successful test command:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

Expected:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
```

Latest package checks:

```bash
dotnet pack '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\Acp.Net.Process\Acp.Net.Process.csproj' --no-restore --output '\\wsl.localhost\Ubuntu\home\mertb\acp-net\artifacts\packages'
dotnet pack '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\Acp.Net.Testing\Acp.Net.Testing.csproj' --no-restore --output '\\wsl.localhost\Ubuntu\home\mertb\acp-net\artifacts\packages'
```

Expected:

```text
Acp.Net.Process: ok
Acp.Net.Testing: ok
```

## Important Findings

1. `AgentClientProtocol` remains useful for protocol-level types and connection behavior.
2. Acp.Net's value is process/runtime/testing/diagnostic behavior around ACP.
3. WSL path mapping is not incidental; real Gemini dogfood exposed it.
4. Tool preflight matters; `rg` missing was detected before agent execution.
5. OpenClaw already has ACPX as runtime backend, so Acp.Net should not replace it.
6. OpenClaw integration should stay reference/diagnostic unless a formal proposal is prepared.

## Current Risks

- API is still alpha-level.
- Diagnostics CLI is intentionally still a sample/tool, not a packaged product.
- Some historical docs contain old absolute paths from the previous OpenClaw workspace location.
- Node child process to Windows interop failed in the Codex sandbox with `UtilBindVsockAnyPort`; this must not be ignored for OpenClaw integration.
- No NuGet publication has happened yet.
- Training Factory remains unproven and should stay out of the MVP path.
- License is Apache-2.0.

## Path Note

The active repository path is now:

```text
/home/mertb/acp-net
```

Some historical spike reports and handoff notes still contain the old path under:

```text
/home/mertb/.openclaw/workspace/acp-net-training-factory
```

Treat those paths as historical evidence from the original spike runs. For current commands, use `README.md`, this file, and `docs/DEVELOPMENT_GUIDE.md`.
