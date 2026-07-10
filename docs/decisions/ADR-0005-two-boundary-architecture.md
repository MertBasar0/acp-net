# ADR-0005: Two-Boundary Architecture — A2A North, ACP/Process South

> 🇹🇷 Türkçe sürüm: [ADR-0005-two-boundary-architecture.tr.md](ADR-0005-two-boundary-architecture.tr.md)

Date: 2026-07-10

## Status

Accepted

## Context

ADR-0004 repositioned Training Factory as an agentic training-ops dogfood field. A follow-up design discussion clarified the original delegation vision: the OpenClaw main model receives a high-level task and delegates it to a worker agent that acts in Isaac Sim — analyzes, starts simulation, runs RL, and keeps looping until the goal is met.

The historical record shows the repository only ever specified the **bottom half** of that vision:

- the `openclaw-subagent-runner` sample encodes delegation as "spawn a local ACP subagent process over stdio, send one prompt, collect one result",
- the never-executed spike plan `002-sb3-learning-baseline.md` (removed from the tree in the docs restructure; recoverable from history at the initial commit) defined the training experiment itself,
- no document specified how the OpenClaw main model reaches a **long-running, goal-looping, possibly remote** worker. A repository-wide search finds no mention of A2A before this ADR.

Choosing ACP for the local process boundary was deliberate and correct (ADR-0001, ECOSYSTEM: the protocol layer was crowded, the process layer empty). The north boundary was simply never designed.

The two protocols solve different boundaries:

- **ACP** (Agent Client Protocol): parent–child, local process, stdio; the worker's lifetime is tied to the session that spawned it. Wrong shape for a multi-day RL loop.
- **A2A** (Agent2Agent): peer-to-peer over HTTP, discoverable via agent card, task lifecycle states, long-running async tasks; the worker can live on a different machine (e.g. the GPU workstation).

An official A2A .NET SDK exists (`A2A` and `A2A.AspNetCore` on NuGet, `1.0.0-preview2`), so the protocol surface is a dependency, not something to build.

## Decision

1. **North boundary = A2A.** The OpenClaw main model acts as an A2A client; the Training-Ops Agent is an A2A server publishing an agent card and task lifecycle. Use the official A2A .NET SDK; do not implement protocol machinery.
2. **The Training-Ops Agent is a separate, deliberately thin application in its own repository.** It is the real form of Training Factory. Its own code is limited to: the A2A surface, a machine-readable job spec schema (YAML/JSON, OSMO-style), the goal loop (analyze → configure → launch → evaluate → decide → iterate, with persistent state between iterations), and run-artifact comparison/eval reporting.
3. **South boundary is unchanged.** Inside the worker, Isaac Lab / RL training / tooling run as local processes launched and diagnosed through `Acp.Net.Process` (preflight, WSL path mapping, transcripts, run artifacts, failure classification); local code agents, when needed, are spawned over ACP.
4. **This repository stays a package family.** Roadmap Phase 5 is the acp-net-side projection of the spike (what dogfooding surfaces as library gaps), not the home of the worker.

```
OpenClaw main model                    intent, task definition, supervision
        │  A2A (agent card, task states, long-running, remote-capable)
        ▼
Training-Ops Agent (own repo)          owns the loop: analyze → sim → RL → eval → repeat
        │  ACP (local code agents)     │  plain processes (Isaac Lab, RL training)
        └──── both cross the Acp.Net process boundary ────┘
              (preflight, WSL paths, transcripts, run artifacts, failure classes)
```

## Consequences

- Acp.Net is not devalued by the A2A move; it keeps operating at the boundary it was written for, inside the worker.
- The A2A investment is shared with the Deliberation Lab project, which targets the same protocol.
- The worker being .NET makes Acp.Net dogfooding structural rather than optional; Python stays at the leaf processes.
- The never-executed spike 002 plan is superseded by the Phase 5 spike definition in ADR-0004, now with the A2A surface added.

## Revisit When

- the A2A .NET SDK changes shape before 1.0 stable in a way that affects the server surface,
- the Phase 5 spike shows the A2A task lifecycle cannot express the training loop's states,
- OpenClaw gains first-class A2A client support that changes the integration shape,
- the worker grows responsibilities that belong in a package (then extract, don't inflate the app).
