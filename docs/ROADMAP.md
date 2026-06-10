# Roadmap

> 🇹🇷 Türkçe sürüm: [ROADMAP.tr.md](ROADMAP.tr.md)

Last updated: 2026-06-10

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
- preserve spike history (now maintained as local engineering notes outside the repository; durable outcomes live in `docs/decisions/`)

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

Goals:

- write `docs/integrations/openclaw.md`
- keep OpenClaw integration as third-party/reference sample
- document how an OpenClaw health check could call the diagnostic command
- avoid runtime backend replacement
- prepare maintainer-friendly proposal only after package is stable

## Phase 4: Real Agent Dogfood

Goals:

- run controlled Gemini ACP dogfood without Claude quota
- preserve transcripts as optional local evidence
- verify custom `--command` diagnostics path
- test missing/available tool scenarios

## Phase 5: Training Factory Revisit

Training Factory remains parked.

Only revisit after Acp.Net alpha packaging is clear.

Next useful Training Factory spike:

- not random rollout
- use a real SB3 learning baseline
- prove whether learning improves over baseline

## Explicit Non-Goals For Now

- full ACP protocol SDK replacement
- OpenClaw core PR
- replacing ACPX
- Training Factory productization
- UI/dashboard
- provider marketplace
