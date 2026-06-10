# ADR-0003: Keep Diagnostics As Tooling For Now

Date: 2026-06-10

## Status

Accepted

## Context

Acp.Net now has OpenClaw-oriented diagnostic probe and doctor/lint mapping drafts. These are useful evidence, but they are not yet stable enough to publish as a separate NuGet package.

The current stable package candidates are:

- `Acp.Net.Process`
- `Acp.Net.Testing`

## Decision

Do not create `Acp.Net.Diagnostics` for the first alpha package round.

Keep diagnostics as repository tooling and samples:

- `src/samples/openclaw-acpnet-probe/`
- `src/openclaw-probe/`
- `docs/contracts/openclaw-doctor-adapter-draft.md`
- `docs/contracts/openclaw-doctor-mapping.scenarios.json`

The diagnostic command contract should remain intentionally small:

- stdout emits one machine-readable JSON result
- stderr is diagnostic/help text only
- exit codes remain stable enough for tool callers
- run artifacts and transcripts remain local evidence
- OpenClaw mapping remains an adapter draft, not core integration

## Consequences

- First alpha package scope stays tighter.
- Diagnostics can evolve without NuGet compatibility pressure.
- OpenClaw integration remains opt-in and external.
- A future `Acp.Net.Diagnostics` package is still possible after repeated usage proves the contract.

## Revisit When

- at least two non-sample consumers need the same diagnostic command contract,
- OpenClaw integration shape is clearer,
- exit codes and JSON result schema have stopped changing,
- diagnostics has tests comparable to `Acp.Net.Process` and `Acp.Net.Testing`.
