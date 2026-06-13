# Current Status

> 🇹🇷 Türkçe sürüm: [CURRENT_STATUS.tr.md](CURRENT_STATUS.tr.md)

Last updated: 2026-06-10

This document is the single source of truth for project state. Other documents link here instead of repeating status information.

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
- native/WSL runtime resolution (routes a bare command that resolves only to a Windows Store execution-alias stub through WSL instead of hanging)
- Windows/WSL path mapping
- `AcpProcessSession.ToAgentPath(...)`
- environment variable shaping
- `AdditionalPathEntries`
- required/optional executable preflight (Windows Store execution-alias stubs are reported as missing, not found)
- warning vs fail-fast preflight policy
- `AcpPreflightException`
- run failure classification
- transcript recording
- run artifact JSON
- graceful shutdown then hard kill
- cleanup of an already-started process when start is cancelled
- deterministic fake ACP agent
- hanging fake agent fixture
- transcript assertions
- OpenClaw-style diagnostic probe with automated exit-code contract tests
- OpenClaw doctor/lint mapping draft

## Verification

From the repository root:

```bash
dotnet test src/acp-net/AcpNetMvp.slnx --logger "console;verbosity=minimal"
dotnet pack src/acp-net/Acp.Net.Process/Acp.Net.Process.csproj --output artifacts/packages
dotnet pack src/acp-net/Acp.Net.Testing/Acp.Net.Testing.csproj --output artifacts/packages
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

All tests should pass, both packages should pack successfully, and the adapter verifier should report `doctor adapter scenarios ok (4)`.

On Windows + WSL setups, see [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) for how to pass WSL paths to a Windows `dotnet.exe`.

## NuGet State

First alpha packages were published on 2026-06-11 through the `publish.yml` workflow using nuget.org trusted publishing (GitHub Actions OIDC):

- [Acp.Net.Process 0.1.0-alpha.1](https://www.nuget.org/packages/Acp.Net.Process)
- [Acp.Net.Testing 0.1.0-alpha.1](https://www.nuget.org/packages/Acp.Net.Testing)

Symbol packages (`.snupkg`) were pushed alongside. Package metadata carries the correct repository/project URL and the Apache-2.0 license expression.

`0.1.0-alpha.2` was published on 2026-06-13. It fixes a Windows trap surfaced by a fresh-consumer test: a bare command such as `python3` resolves to a Microsoft Store execution-alias stub, which preflight reported as found and the runner then launched, hanging silently. The runtime resolver now routes such a command through WSL, and preflight reports the stub as missing instead.

Future releases go through the same manually triggered workflow; see [RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md) for the gate list.

## Important Findings

1. `AgentClientProtocol` remains useful for protocol-level types and connection behavior.
2. Acp.Net's value is process/runtime/testing/diagnostic behavior around ACP.
3. WSL path mapping is not incidental; real Gemini dogfood exposed it.
4. Tool preflight matters; a missing `rg` was detected before agent execution.
5. OpenClaw already has ACPX as runtime backend, so Acp.Net should not replace it.
6. OpenClaw integration should stay reference/diagnostic unless a formal proposal is prepared.

## Current Risks

- API is still alpha-level.
- Diagnostics CLI is intentionally still a sample/tool, not a packaged product.
- On some Windows + WSL setups, a Node child process that calls Windows interop executables can fail at the WSL interop boundary (`UtilBindVsockAnyPort`); this path needs dedicated verification before any OpenClaw integration work.

## Engineering Notes Archive

Dated spike reports (001–014) and daily handoff notes were moved out of the git history on 2026-06-10. They live in the untracked `notes/` folder at the repository root (`notes/handoffs/`, `notes/spikes/`); the folder is ignored by git and never pushed to the GitHub remote. The durable decisions distilled from them live in [decisions/](decisions/).
