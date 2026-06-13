# Ecosystem Positioning

> 🇹🇷 Türkçe sürüm: [ECOSYSTEM.tr.md](ECOSYSTEM.tr.md)

Last reviewed: 2026-06-13 (NuGet metadata + project READMEs).

This note records how Acp.Net sits next to the other .NET packages for ACP
(the Agent Client Protocol). It is a point-in-time scan, not a live feed; numbers
will drift.

## The .NET ACP landscape

A NuGet search for ACP-protocol packages surfaces a small but growing set. The
ones that implement the Agent Client Protocol (not the unrelated "ACP" products
such as Tencent Cloud ACP or document-automation wrappers):

| Package | Owner | Latest | First published | Scope |
| --- | --- | --- | --- | --- |
| [`AgentClientProtocol`](https://www.nuget.org/packages/AgentClientProtocol) | nuskey8 | 0.1.5 | 2025-11 | Typed protocol/JSON-RPC SDK (client + agent) |
| [`dotacp.protocol` / `.agent` / `.client`](https://www.nuget.org/packages/dotacp.protocol) | timxx | 2026.5.10 | 2026-05 | Schema-generated protocol + JSON-RPC trio (uses StreamJsonRpc) |
| [`Acp.Sdk`](https://www.nuget.org/packages/Acp.Sdk) | acp-sdk | 0.1.0 | 2026-04 | Agent-building SDK (early, single release) |
| [`LibAcp`](https://www.nuget.org/packages/LibAcp) | sargeMonkey | 0.1.0 | 2026-05 | JSON-RPC 2.0 + ndjson transport over a caller stream (early) |
| **`Acp.Net.Process` / `Acp.Net.Testing`** | this project | 0.1.0-alpha.2 | 2026-06 | Process/runtime/testing layer (no protocol schema) |

## The key observation

Every other package operates at the **protocol/SDK layer**: it models ACP types
and runs JSON-RPC over a stream that the caller provides. Their own docs say so
explicitly — `dotacp` lists "process spawning, Windows-to-WSL bridging, path
mapping, environment/PATH, executable preflight, stdio transcript, run artifacts"
as **left to the consumer**, and `LibAcp`'s quick start tells the caller to
"spawn the agent process (via `ProcessStartInfo`), wire up stdio redirection,
and construct the stream."

That consumer-owned glue code is exactly what Acp.Net provides. The protocol
layer is crowded; the process/runtime layer is empty.

## Overlap and divergence

- **Overlap: minimal.** Acp.Net deliberately does not own protocol types
  (see [decisions/ADR-0001](decisions/ADR-0001-incumbent-comparison-decision.md)),
  and the protocol packages deliberately do not own process/runtime. The two
  layers are orthogonal.
- **Divergence: the whole value.** Acp.Net's surface — process launch, Windows/WSL
  bridge, path mapping, environment/PATH shaping, executable preflight, transcript
  recording, run artifacts, failure classification, shutdown policy, and
  process-boundary testing with fake agents — is precisely what the protocol
  packages mark out of scope.

## What this means for positioning

1. **Acp.Net is protocol-package-agnostic.** It produces the agent's stdio
   streams and hands them to whichever protocol package the consumer prefers
   (`AgentClientProtocol`, `dotacp`, `LibAcp`, …). `Acp.Net.Process` takes no
   dependency on any of them; the samples happen to use `AgentClientProtocol`.
   This makes the full user base of every protocol package a potential consumer.
2. **The ADR-0001 "narrow" decision held up.** Since that decision the protocol
   layer grew to five packages while the runtime layer stayed a category of one.
3. **Traction is honest.** `AgentClientProtocol` (~2.3k downloads) and `dotacp`
   (~850) are ahead on adoption; Acp.Net is alpha. The advantage is being the
   only package in its niche, not being the most downloaded.

## Watch list

- `dotacp` is the most complete neighbor (schema-generated, broad target
  frameworks, active). If it ever grows process/runtime helpers, the overlap
  question reopens — worth re-checking on each release.
- `Acp.Sdk` describes itself as an agent-building SDK; if it expands toward
  runtime orchestration, re-evaluate.
