# Roadmap

> 🇹🇷 Türkçe sürüm: [ROADMAP.tr.md](ROADMAP.tr.md)

Last updated: 2026-07-09

## Product Goal

Ship Acp.Net as an independent .NET package family:

- `Acp.Net.Process`
- `Acp.Net.Testing`

Diagnostics remains repository tooling for now. A future `Acp.Net.Diagnostics` package is possible, but it is not part of the first alpha package round.

OpenClaw should remain a reference integration, not the default implementation target.

## Phase 0: Repository Independence

Status: complete.

Goals:

- move project out of OpenClaw workspace
- initialize git
- push to standalone GitHub repo
- document current status and next steps
- preserve spike history (now kept in the untracked `notes/` folder at the repository root; durable outcomes live in `docs/decisions/`)

## Phase 1: Alpha Package Hardening

Goals:

- review public API naming
- decide package versioning policy
- add package README coverage
- add XML docs or minimal API docs
- ensure `dotnet test` and `dotnet pack` are one-command reproducible
- add CI workflow
- license selected: Apache-2.0

Candidate output:

- `Acp.Net.Process.0.1.0-alpha.1`
- `Acp.Net.Testing.0.1.0-alpha.1`

## Phase 2: Diagnostics Shape

Goals:

- keep diagnostics as sample/tooling for now
- stabilize `openclaw-acpnet-probe` CLI contract
- keep stdout as one JSON result
- keep stderr diagnostic-only
- finalize exit codes
- document doctor/lint mapping

Do not add OpenClaw core code in this phase.

Do not create `Acp.Net.Diagnostics` in this phase.

## Phase 3: Reference Integrations

Status: reference guide written ([docs/integrations/openclaw.md](integrations/openclaw.md)). The remaining goals are ongoing/conditional.

Goals:

- write `docs/integrations/openclaw.md` — done
- keep OpenClaw integration as third-party/reference sample
- document how an OpenClaw health check could call the diagnostic command — done
- avoid runtime backend replacement
- prepare maintainer-friendly proposal only after package is stable

## Phase 4: Real Agent Dogfood

Status: done (2026-06-13). Dogfooded against the real Gemini CLI ACP agent; both
the sample and the probe `--command` path complete end to end. The run surfaced
one real code bug — the probe's `--arg` parser rejected flag-style values like
`--acp` (fixed) — plus an environment issue: the installed Gemini CLI was an old
build (0.36, predating `--skip-trust`) rather than a current release. The fix for
that is to run a current Gemini CLI, not to change the sample.

Goals:

- run controlled real-agent verification with the Gemini CLI ACP mode — done
- preserve transcripts as optional local evidence — done (gitignored `artifacts/`)
- verify custom `--command` diagnostics path — done
- test missing/available tool scenarios — done

## Phase 5: Training Factory Spike (Isaac Lab)

Status: planned, gated on local GPU availability. Direction set in [ADR-0004](decisions/ADR-0004-training-factory-agentic-training-ops.md).

Goals:

- express one training job as a machine-readable spec (YAML/JSON, OSMO-style)
- run "start training → run eval → report run artifact" end to end from one OpenClaw command
- route launch/preflight/diagnostics through Acp.Net wherever .NET is in the loop
- classify any failed run into the Acp.Net failure classes
- prove repeatability: the same command twice yields comparable run artifacts

Do not start a new orchestrator codebase in this phase.

## Explicit Non-Goals For Now

- full ACP protocol SDK replacement
- OpenClaw core PR
- replacing ACPX
- UI/dashboard
- provider marketplace
- a horizontal training orchestrator competing with NVIDIA OSMO (see [ADR-0004](decisions/ADR-0004-training-factory-agentic-training-ops.md))
