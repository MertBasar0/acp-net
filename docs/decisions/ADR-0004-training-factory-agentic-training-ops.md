# ADR-0004: Training Factory Is An Agentic Training-Ops Dogfood Field

> 🇹🇷 Türkçe sürüm: [ADR-0004-training-factory-agentic-training-ops.tr.md](ADR-0004-training-factory-agentic-training-ops.tr.md)

Date: 2026-07-09

## Status

Accepted

## Context

ADR-0000 shelved Training Factory as a test/dogfooding area for Acp.Net and defined its next experiment as a real SB3 learning baseline in a Gazebo/PX4-oriented environment.

Since then, two things changed.

On the Acp.Net side:

- `Acp.Net.Process` and `Acp.Net.Testing` are published on NuGet (`0.1.0-alpha.2`),
- Phase 4 real-agent dogfood (Gemini CLI ACP mode) is complete.

On the landscape side:

- NVIDIA open-sourced OSMO, a production-grade orchestration platform for Physical AI workloads (Isaac Lab, Isaac Sim, GR00T), with YAML-based multi-stage workflows across heterogeneous compute.
- OSMO now integrates with coding agents (Claude Code, Codex, Cursor) so that agents can manage training operations directly.
- Isaac Lab covers the environment + training stack (RSL-RL, SKRL, RL-Games, Stable-Baselines3) with multi-GPU scaling, headless operation, and a published reference architecture.

The product vision behind Training Factory — give an orchestrator such as OpenClaw a high-level command and have it manage simulation, RL training, and evaluation for robotics projects — is therefore validated by the market and simultaneously occupied by an incumbent.

## Decision

1. Training Factory is not a horizontal product. Do not build a new training orchestrator that competes with OSMO.
2. Redefine Training Factory as an **agentic training-ops dogfood field**: a thin, agent-facing layer on top of the existing stack — Isaac Lab as environment/training, OSMO-style machine-readable job specs as the workflow model, OpenClaw as the commanding agent, Acp.Net at the process boundary.
3. Retire the ADR-0000 next experiment (real SB3 baseline on Gazebo/PX4). Replace it with an Isaac Lab based spike (below).
4. Keep the differentiating focus on the Windows workstation + WSL bridge: preflight, path mapping, transcript recording, run artifact JSON — the platform dirt a data-center orchestrator does not care about.

## Spike Definition

Target effort: 2–4 weeks of part-time work, gated on local GPU availability.

- environment: Isaac Lab, headless, running locally (WSL or native Linux)
- training: one small baseline task with one supported RL library
- orchestration: OpenClaw issues a single high-level command; the job is expressed as a machine-readable spec (YAML/JSON, OSMO-style)
- process boundary: launch, preflight, and diagnostics go through Acp.Net wherever .NET is in the loop (probe path)
- output: run artifact JSON + eval report + transcript

Success criteria:

- the loop "start training → run eval → report run artifact" completes end to end from one command,
- a failed run is classifiable into the Acp.Net failure classes (environment / process / protocol / agent-task),
- repeating the same command produces a comparable run artifact (repeatability over peak score).

## Consequences

- No new orchestrator codebase is started.
- Training Factory has no productization timeline; the ADR-0000 estimate (6–12 months, high integration risk) is retired rather than rescheduled.
- Planned local multi-GPU hardware (e.g. dual RTX 5090) increases the value of the queue/eval/report loop but does not change this decision.
- The work retains portfolio value as an independent exploration of the same agent-driven training-ops direction OSMO productized.

## Revisit When

- local multi-GPU hardware actually arrives,
- the Isaac Lab spike completes and produces a repeatable run artifact,
- OSMO's agent integration demonstrably covers (or fails to cover) Windows workstation + WSL workflows,
- a second real consumer wants the same thin agent-facing layer.
