# OpenClaw Integration (Reference)

> 🇹🇷 Türkçe sürüm: [openclaw.tr.md](openclaw.tr.md)

This is a reference integration guide, not a shipped OpenClaw feature. It shows
how OpenClaw could call Acp.Net as an **external diagnostic command** and surface
the result through its doctor/lint health checks. The strategy behind it is in
[OPENCLAW_INTEGRATION_STRATEGY.md](../OPENCLAW_INTEGRATION_STRATEGY.md);
the decisions are [ADR-0002](../decisions/ADR-0002-independent-package-openclaw-reference.md)
and [ADR-0003](../decisions/ADR-0003-diagnostics-remains-tooling.md).

## What this is — and is not

- **Is:** an opt-in, third-party diagnostic. OpenClaw runs a command, reads one
  JSON result, and maps it to health findings. Acp.Net supplies environment /
  preflight / failure evidence that ACPX does not surface on its own.
- **Is not:** an ACPX replacement. OpenClaw already owns ACP session/turn/event
  runtime and process-lease cleanup in `extensions/acpx`. Acp.Net does not run
  OpenClaw turns and adds no required .NET dependency to OpenClaw core.

The distinction Acp.Net brings to a health check is the one ACPX does not draw
on its own:

> environment failure ≠ agent failure ≠ process failure ≠ protocol failure.

## Integration shape

```
OpenClaw doctor / health check
        │  spawn external command
        ▼
openclaw-acpnet-probe  ──►  one JSON result on stdout  (+ exit code)
        │
        ▼
adapter maps result ──►  AcpRuntimeDoctorReport / HealthFinding[]
```

The probe is the stabilized sample at
[`src/samples/openclaw-acpnet-probe/`](../../src/samples/openclaw-acpnet-probe/).
By default it runs a deterministic fake ACP agent (no LLM, no model quota), so a
health check can run it on every `doctor` invocation without cost.

## The diagnostic command contract

Run the default probe (fake agent):

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj
```

Point it at a real ACP-compatible agent instead:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj -- \
  --agent gemini \
  --command /path/to/gemini \
  --arg --acp \
  --required-tool git \
  --optional-tool rg
```

Contract guarantees:

- **stdout** carries exactly one JSON result (a `openclaw.acpnet.probe.result`
  object). Nothing else is written to stdout.
- **stderr** is diagnostic/help text only.
- **exit codes** are stable for tool callers:

  | Code | Meaning |
  | --- | --- |
  | `0` | `ok=true` |
  | `2` | environment / preflight failure |
  | `3` | runtime / protocol / agent / unknown failure |
  | `64` | invalid CLI configuration |

A successful result looks like this (see
[contracts/openclaw-acpnet-probe-result.example.json](../contracts/openclaw-acpnet-probe-result.example.json)):

```json
{
  "kind": "openclaw.acpnet.probe.result",
  "ok": true,
  "result": "completed",
  "failureKind": "None",
  "usesWsl": true,
  "runArtifactPath": ".../probe-run.json",
  "transcriptPath": ".../probe-transcript.ndjson",
  "preflight": {
    "criticalMissing": [],
    "warnings": [{ "name": "rg", "found": false, "missingPolicy": "Warn" }],
    "tools": []
  }
}
```

## Mapping the result to OpenClaw surfaces

The full mapping — to `AcpRuntimeDoctorReport` and `HealthFinding[]` — is defined
in [contracts/openclaw-doctor-adapter-draft.md](../contracts/openclaw-doctor-adapter-draft.md),
with machine-checkable scenarios in
[contracts/openclaw-doctor-mapping.scenarios.json](../contracts/openclaw-doctor-mapping.scenarios.json)
and an executable adapter draft plus verifier in
[`src/openclaw-probe/`](../../src/openclaw-probe/). Run the verifier with:

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
# doctor adapter scenarios ok (4)
```

The mapping in one table:

| Probe result | Runtime doctor | Lint finding |
| --- | --- | --- |
| `ok`, no warnings | `ok: true` | none |
| `ok`, optional tool missing | `ok: true`, `ACPNET_PREFLIGHT_WARNING` | `warning` |
| environment failure (exit 2) | `ok: false`, `ACPNET_ENVIRONMENT_FAILURE` | `error` |
| configuration failure (exit 64) | `ok: false`, `ACPNET_PROBE_CONFIG_INVALID` | `error` |
| process/protocol/agent failure (exit 3) | `ok: false`, `ACPNET_PROBE_FAILED` | `error` |

A missing **optional** tool keeps the runtime doctor green (`ok: true`) but
should still raise a `warning` finding on the lint surface; a missing **critical**
tool is an error on both.

## A minimal OpenClaw health check (sketch)

```ts
import { mapProbeResultToDoctorReport } from "./acpnet-doctor-adapter";

async function acpnetHealthCheck(): Promise<AcpRuntimeDoctorReport> {
  const { stdout, exitCode } = await runCommand("openclaw-acpnet-probe", []);
  const result = JSON.parse(stdout); // exactly one JSON object
  return mapProbeResultToDoctorReport(result, exitCode);
}
```

The adapter functions (`mapProbeResultToDoctorReport`,
`mapProbeResultToHealthFindings`) already exist as a draft in
[`src/openclaw-probe/doctor-adapter-draft.mjs`](../../src/openclaw-probe/doctor-adapter-draft.mjs).

## Runtime boundary caveat

When OpenClaw's Node runtime spawns a command that crosses into Windows interop
from inside WSL, that path needs verification in OpenClaw's **actual** runtime.
A sandboxed probe hit `UtilBindVsockAnyPort` at the Node-child-process →
Windows-interop boundary, while the same command ran fine directly from a shell.
Before wiring the probe into OpenClaw, decide how to run it:

- as a host-side command (Windows host process), or
- a WSL-native `dotnet` install, or
- an approved external command outside the sandbox.

This boundary is the main open question for a real integration, not the mapping.

## Maturity and a maintainer-friendly path

This stays a reference/sample until the command contract has more usage evidence
(per ADR-0003). If an OpenClaw PR is ever proposed it should be small and
integration-point-oriented: no broad runtime rewrite, no required .NET dependency
in core, no ACPX replacement, all behavior behind opt-in config or a plugin/sample
path.

## Artifacts index

- Probe sample: [`src/samples/openclaw-acpnet-probe/`](../../src/samples/openclaw-acpnet-probe/)
- Node command-wrapper + adapter draft + verifier: [`src/openclaw-probe/`](../../src/openclaw-probe/)
- Mapping contract: [contracts/openclaw-doctor-adapter-draft.md](../contracts/openclaw-doctor-adapter-draft.md)
- Mapping scenarios: [contracts/openclaw-doctor-mapping.scenarios.json](../contracts/openclaw-doctor-mapping.scenarios.json)
- Result/artifact examples: [contracts/openclaw-acpnet-probe-result.example.json](../contracts/openclaw-acpnet-probe-result.example.json), [contracts/acpnet-run-artifact.example.json](../contracts/acpnet-run-artifact.example.json)
- Strategy & decisions: [OPENCLAW_INTEGRATION_STRATEGY.md](../OPENCLAW_INTEGRATION_STRATEGY.md), [ADR-0002](../decisions/ADR-0002-independent-package-openclaw-reference.md), [ADR-0003](../decisions/ADR-0003-diagnostics-remains-tooling.md)
