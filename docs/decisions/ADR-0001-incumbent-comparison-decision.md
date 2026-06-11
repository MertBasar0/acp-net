# ADR-0001: Acp.Net Incumbent Comparison Decision

> 🇹🇷 Türkçe sürüm: [ADR-0001-incumbent-comparison-decision.tr.md](ADR-0001-incumbent-comparison-decision.tr.md)

Date: 2026-06-07

## Status

The `AgentClientProtocol` NuGet package and the Acp.Net-style process bridge approach were compared against the same mock ACP agent.

Package used:

- `AgentClientProtocol` 0.1.5
- NuGet: https://www.nuget.org/packages/AgentClientProtocol
- Source repo: https://github.com/nuskey8/acp-csharp

Working spike code:

`src/spikes/acp-incumbent-comparison/`

## Decision

Decision: `narrow`.

Acp.Net must not be positioned as a full protocol SDK competing with the `AgentClientProtocol` package.

Acp.Net's initial value area is narrowed to:

- stdio process lifecycle
- Windows/WSL path and runtime bridge
- stdout/stderr separation
- timeout, graceful stop, hard kill
- raw transcript/debug helpers
- fake ACP server and test assertion helpers

## Rationale

The `AgentClientProtocol` package worked well on the typed protocol/schema side:

- `initialize`
- `session/new`
- `session/prompt`
- streaming `session/update`
- `session/cancel`

Rewriting that surface does not justify the maintenance cost.

However, in the package's own samples and in the executed spike, the following concerns were left to application code:

- starting the agent process
- draining stderr
- process shutdown strategy
- crossing from a Windows `dotnet.exe` into a WSL `python3` agent
- UNC path / WSL path conversion
- transcript/debug recording
- test harness ergonomics

Previous spikes also spent most of their time in these areas. The value is therefore in the platform/process layer, not the protocol model.

## Outcome

The next technical work is designing the Acp.Net MVP with a narrow scope:

- `Acp.Net.Process`
- `Acp.Net.Testing`
- a stdio harness that can work together with the existing `AgentClientProtocol` package

## Evidence

The comparison score table and run transcripts are recorded in the spike session reports (spike 001), kept in the untracked `notes/` folder at the repository root (ignored by git, not pushed to the remote). The executable comparison code remains in `src/spikes/acp-incumbent-comparison/`.
