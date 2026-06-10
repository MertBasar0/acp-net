# OpenClaw Integration Strategy

> 🇹🇷 Türkçe sürüm: [OPENCLAW_INTEGRATION_STRATEGY.tr.md](OPENCLAW_INTEGRATION_STRATEGY.tr.md)

Last updated: 2026-06-10

## Decision

Do not treat OpenClaw core integration as the main product path.

Acp.Net should be developed as an independent package family. OpenClaw remains a reference consumer and dogfood target.

## Why

OpenClaw already has `extensions/acpx` for ACP runtime backend behavior:

- session creation
- turn streaming
- runtime events
- process lease state
- cleanup/reaper behavior
- doctor hooks

Acp.Net should not replace ACPX.

Acp.Net's distinct value is:

- process runtime evidence
- preflight checks
- WSL/path mapping
- transcript recording
- run artifact JSON
- failure classification
- deterministic process-boundary testing

## Acceptable Integration Shapes

### Preferred: Third-Party Reference Integration

Provide documentation and samples showing how OpenClaw could call an Acp.Net diagnostic command.

This keeps user choice and avoids forcing .NET into OpenClaw core.

### Acceptable Later: Doctor/Lint Adapter Proposal

Only after package stabilization, propose a small adapter path:

- call external diagnostic command
- parse one JSON result
- map it to `HealthFinding[]`
- do not replace ACPX

### Not Recommended: ACPX Replacement

This would duplicate existing OpenClaw runtime responsibilities and create a difficult maintainer burden.

## Maintainer-Friendly Rule

If an OpenClaw PR is ever proposed, it should be small and integration-point oriented:

- no broad runtime rewrite
- no required .NET dependency in core
- no ACPX replacement
- no hidden process behavior changes
- all behavior behind opt-in config or plugin/sample path

## Current Reference Artifacts

In this repository:

- `src/samples/openclaw-acpnet-probe/` — stabilized diagnostic command probe
- `src/openclaw-probe/doctor-adapter-draft.mjs` — doctor/lint adapter draft
- `src/openclaw-probe/verify-doctor-adapter-draft.mjs` — adapter scenario verifier
- `docs/contracts/openclaw-doctor-adapter-draft.md` — mapping contract
- `docs/contracts/openclaw-doctor-mapping.scenarios.json` — scenario fixtures

The dated spike reports (011–014) that produced these artifacts are maintained as the maintainer's local engineering notes outside this repository. Their durable conclusions are recorded in [decisions/ADR-0002](decisions/ADR-0002-independent-package-openclaw-reference.md) and [decisions/ADR-0003](decisions/ADR-0003-diagnostics-remains-tooling.md).
