# OpenClaw Doctor Adapter Draft

Tarih: 2026-06-09

Bu dosya Spike 014 icin Acp.Net diagnostic command sonucunun OpenClaw doctor/lint yuzeylerine nasil map edilecegini tanimlar.

## Kaynak Contract

Kaynak komut:

`src/samples/openclaw-acpnet-probe/`

Komut stdout'a tek JSON result basar:

```json
{
  "kind": "openclaw.acpnet.probe.result",
  "ok": true,
  "result": "completed",
  "failureKind": "None",
  "failureMessage": null,
  "agentName": "openclaw-fake-acp-subagent",
  "usesWsl": true,
  "runArtifactPath": ".../probe-run.json",
  "transcriptPath": ".../probe-transcript.ndjson",
  "preflight": {
    "criticalMissing": [],
    "warnings": [],
    "tools": []
  }
}
```

Exit code sozlesmesi:

```text
0   ok=true
2   environment/preflight failure
3   runtime/protocol/agent/unknown failure
64  invalid CLI configuration
```

## Hedef OpenClaw Yuzeyleri

### Runtime Doctor Report

OpenClaw ACP runtime tarafinda uygun model:

```ts
type AcpRuntimeDoctorReport = {
  ok: boolean;
  code?: string;
  message: string;
  installCommand?: string;
  details?: string[];
};
```

### Structured Health Finding

OpenClaw doctor lint tarafinda uygun model:

```ts
interface HealthFinding {
  checkId: string;
  severity: "info" | "warning" | "error";
  message: string;
  source?: string;
  target?: string;
  requirement?: string;
  fixHint?: string;
}
```

## Mapping Kurallari

### Basarili Run

Kosul:

```text
ok=true
failureKind=None
criticalMissing=[]
warnings=[]
```

Doctor report:

```json
{
  "ok": true,
  "message": "Acp.Net diagnostic probe completed."
}
```

Health finding:

- finding yok.

### Basarili Ama Warning Var

Kosul:

```text
ok=true
warnings.length > 0
```

Doctor report:

```json
{
  "ok": true,
  "code": "ACPNET_PREFLIGHT_WARNING",
  "message": "Acp.Net diagnostic probe completed with optional runtime warnings.",
  "details": ["Optional tool missing: rg (which exited 1)"]
}
```

Health finding:

```json
{
  "checkId": "plugin/acpnet/diagnostic-probe",
  "severity": "warning",
  "message": "Acp.Net optional runtime tool is missing: rg",
  "source": "acpnet",
  "target": "rg",
  "requirement": "optional executable",
  "fixHint": "Install rg in the agent runtime or pass it through PATH/AdditionalPathEntries."
}
```

### Environment Failure

Kosul:

```text
exitCode=2
failureKind=EnvironmentFailure
criticalMissing.length > 0
```

Doctor report:

```json
{
  "ok": false,
  "code": "ACPNET_ENVIRONMENT_FAILURE",
  "message": "Acp.Net diagnostic probe failed before the agent started.",
  "details": ["Critical tool missing: git (which exited 1)"]
}
```

Health finding:

```json
{
  "checkId": "plugin/acpnet/diagnostic-probe",
  "severity": "error",
  "message": "Acp.Net critical runtime tool is missing: git",
  "source": "acpnet",
  "target": "git",
  "requirement": "critical executable",
  "fixHint": "Install git in the agent runtime or configure the probe with a valid PATH."
}
```

### Configuration Failure

Kosul:

```text
exitCode=64
failureKind=ConfigurationFailure
```

Doctor report:

```json
{
  "ok": false,
  "code": "ACPNET_PROBE_CONFIG_INVALID",
  "message": "Acp.Net diagnostic probe configuration is invalid.",
  "details": ["--arg requires --command."]
}
```

Health finding:

```json
{
  "checkId": "plugin/acpnet/diagnostic-probe",
  "severity": "error",
  "message": "Acp.Net diagnostic probe configuration is invalid: --arg requires --command.",
  "source": "acpnet",
  "requirement": "valid probe configuration",
  "fixHint": "Fix the Acp.Net probe command arguments."
}
```

### Process / Protocol / Agent / Unknown Failure

Kosul:

```text
exitCode=3
failureKind in ProcessFailure | ProtocolFailure | AgentFailure | Unknown
```

Doctor report:

```json
{
  "ok": false,
  "code": "ACPNET_PROBE_FAILED",
  "message": "Acp.Net diagnostic probe failed.",
  "details": ["<failureMessage>", "Transcript: <transcriptPath>", "Run artifact: <runArtifactPath>"]
}
```

Health finding:

```json
{
  "checkId": "plugin/acpnet/diagnostic-probe",
  "severity": "error",
  "message": "Acp.Net diagnostic probe failed: <failureMessage>",
  "source": "acpnet",
  "requirement": "successful diagnostic probe",
  "fixHint": "Inspect the run artifact and transcript paths reported by the probe."
}
```

## Adapter Pseudocode

```ts
function mapProbeResultToDoctorReport(result, exitCode): AcpRuntimeDoctorReport {
  if (result.ok && result.preflight.warnings.length === 0) {
    return { ok: true, message: "Acp.Net diagnostic probe completed." };
  }

  if (result.ok) {
    return {
      ok: true,
      code: "ACPNET_PREFLIGHT_WARNING",
      message: "Acp.Net diagnostic probe completed with optional runtime warnings.",
      details: result.preflight.warnings.map(formatToolWarning),
    };
  }

  if (exitCode === 2 || result.failureKind === "EnvironmentFailure") {
    return {
      ok: false,
      code: "ACPNET_ENVIRONMENT_FAILURE",
      message: "Acp.Net diagnostic probe failed before the agent started.",
      details: result.preflight.criticalMissing.map(formatCriticalTool),
    };
  }

  if (exitCode === 64 || result.failureKind === "ConfigurationFailure") {
    return {
      ok: false,
      code: "ACPNET_PROBE_CONFIG_INVALID",
      message: "Acp.Net diagnostic probe configuration is invalid.",
      details: [result.failureMessage].filter(Boolean),
    };
  }

  return {
    ok: false,
    code: "ACPNET_PROBE_FAILED",
    message: "Acp.Net diagnostic probe failed.",
    details: [result.failureMessage, result.transcriptPath, result.runArtifactPath].filter(Boolean),
  };
}
```

## Open Questions

1. OpenClaw bu probe'u `doctor --lint` icinde mi, `acpx doctor` icinde mi, yoksa ayri plugin command olarak mi cagirmali?
2. Node runtime -> Windows interop hatasi nasil asilacak?
3. Probe sonucunda warning varken doctor exit code ne olmali?
   - `doctor --lint --severity-min warning` icin finding varsa exit 1.
   - Runtime doctor report icin `ok=true` kalabilir.
4. `installCommand` sadece sistem paketleri icin mi kullanilmali?
   - Ornek: `sudo apt install ripgrep`
   - Ama Windows/WSL ve distro farklari nedeniyle default olarak `fixHint` daha guvenli.

