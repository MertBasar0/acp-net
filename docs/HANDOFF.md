# Handoff

Last updated: 2026-06-10

This repository is the standalone Acp.Net workspace. It was moved out of the OpenClaw workspace so it can evolve as an independent package family.

## Read First

1. `README.md`
2. `docs/CURRENT_STATUS.md`
3. `docs/ROADMAP.md`
4. `docs/DEVELOPMENT_GUIDE.md`
5. `docs/OPENCLAW_INTEGRATION_STRATEGY.md`
6. `docs/decisions/ADR-0002-independent-package-openclaw-reference.md`

## Current Product Thesis

Acp.Net should be a small .NET package family for ACP-compatible local agent process runtime, testing, and diagnostics.

It should not become an OpenClaw fork and should not start with OpenClaw core PRs. OpenClaw is the strongest reference consumer for now, but the product boundary should remain outside OpenClaw.

## What Exists

- `Acp.Net.Process`: process runner, runtime resolution, Windows/WSL path mapping, env shaping, preflight checks, transcript recording, run artifacts, shutdown policy.
- `Acp.Net.Testing`: deterministic fake ACP agents and transcript assertions.
- OpenClaw-oriented probes: sample diagnostics and doctor adapter draft. Diagnostics is not a separate NuGet package yet.
- Spike history: `docs/spikes/`.
- Decisions: `docs/decisions/`.

## Verify Before Continuing

Run:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

Expected:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
doctor adapter scenarios ok (4)
```

Package smoke check:

```bash
dotnet pack '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\Acp.Net.Process\Acp.Net.Process.csproj' --no-restore --output '\\wsl.localhost\Ubuntu\home\mertb\acp-net\artifacts\packages'
dotnet pack '\\wsl.localhost\Ubuntu\home\mertb\acp-net\src\acp-net\Acp.Net.Testing\Acp.Net.Testing.csproj' --no-restore --output '\\wsl.localhost\Ubuntu\home\mertb\acp-net\artifacts\packages'
```

## Next Best Work

1. Review public API names in `Acp.Net.Process`.
2. Decide license.
3. Add minimal API docs/XML comments for public types.
4. Keep diagnostics as repository tooling while the contract settles.
5. Inspect generated alpha packages locally.
6. Prepare alpha packages but do not publish before API and license review.

## NuGet State

Local package generation works for:

- `Acp.Net.Process.0.1.0-alpha.1`
- `Acp.Net.Testing.0.1.0-alpha.1`

Do not publish yet. The package metadata has the correct repository/project URL, but license metadata is intentionally unset until the project owner chooses a license.

## Decisions To Avoid Reopening Too Early

- Do not replace the existing `AgentClientProtocol` package.
- Do not make OpenClaw core integration the first product milestone.
- Do not revive Training Factory until Acp.Net package boundaries are stable.
- Do not treat fake agent success as proof of real model/provider reliability.

## Known Caveats

- Some old spike reports contain the previous path under `.openclaw/workspace/acp-net-training-factory`; those are historical.
- Gemini/real-agent dogfood should be run carefully because it can consume quota.
- The Node-to-Windows interop issue seen in sandboxed OpenClaw probing still needs a real non-sandbox confirmation.
- No NuGet package has been published yet.
