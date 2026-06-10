# ADR-0002: Independent Package, OpenClaw As Reference Consumer

Date: 2026-06-10

## Status

Accepted.

## Context

During spikes 011-014, Acp.Net was evaluated against OpenClaw integration needs.

OpenClaw already has ACPX runtime support in `extensions/acpx`. That runtime owns session/turn semantics, process lease state, and cleanup behavior. Adding Acp.Net directly into OpenClaw core would risk duplicating ACPX responsibilities and would require a difficult maintainer review.

At the same time, Acp.Net has demonstrated value that is not OpenClaw-specific:

- process launch and shutdown
- native/WSL path mapping
- environment and PATH shaping
- required/optional executable preflight
- transcript recording
- run artifact generation
- process-boundary fake agent testing
- diagnostic command output

## Decision

Acp.Net will be developed as an independent .NET package family:

- `Acp.Net.Process`
- `Acp.Net.Testing`

Diagnostics remains repository tooling for now. A possible future `Acp.Net.Diagnostics` package is covered separately by ADR-0003.

OpenClaw will be treated as a reference consumer and dogfood environment, not as the main product boundary.

## Consequences

Positive:

- Acp.Net can be useful outside OpenClaw.
- Users can opt into the package instead of accepting core runtime changes.
- Maintainer burden for OpenClaw is reduced.
- Product messaging is clearer.
- NuGet packaging and docs become the main delivery path.

Negative:

- OpenClaw integration remains a later step.
- Some duplication in sample/adapter code may exist until integration points are formalized.
- Diagnostics command packaging is intentionally deferred.

## Follow-Up

Next work should harden Acp.Net as a package family before any OpenClaw integration PR:

1. finalize README and package docs,
2. clean public API,
3. add CI,
4. decide license,
5. publish alpha packages,
6. keep OpenClaw integration as a sample/proposal.
