# OpenClaw Doctor Adapter Taslağı

> 🇬🇧 English version: [openclaw-doctor-adapter-draft.md](openclaw-doctor-adapter-draft.md)

Tarih: 2026-06-09

Bu dosya, Acp.Net tanılama komutu sonucunun OpenClaw doctor/lint yüzeylerine nasıl eşleneceğini tanımlar.

## Kaynak Kontrat

Kaynak komut:

`src/samples/openclaw-acpnet-probe/`

Komut stdout'a tek bir JSON sonucu basar:

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

Exit code sözleşmesi:

```text
0   ok=true
2   environment/preflight hatası
3   runtime/protokol/agent/bilinmeyen hata
64  geçersiz CLI konfigürasyonu
```

## Hedef OpenClaw Yüzeyleri

### Runtime Doctor Raporu

OpenClaw ACP runtime tarafındaki uygun model:

```ts
type AcpRuntimeDoctorReport = {
  ok: boolean;
  code?: string;
  message: string;
  installCommand?: string;
  details?: string[];
};
```

### Yapılandırılmış Health Finding

OpenClaw doctor lint tarafındaki uygun model:

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

## Eşleme Kuralları

### Başarılı Çalıştırma

Koşul:

```text
ok=true
failureKind=None
criticalMissing=[]
warnings=[]
```

Doctor raporu:

```json
{
  "ok": true,
  "message": "Acp.Net diagnostic probe completed."
}
```

Health finding:

- finding yok.

### Uyarılı Başarı

Koşul:

```text
ok=true
warnings.length > 0
```

Doctor raporu:

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

### Environment Hatası

Koşul:

```text
exitCode=2
failureKind=EnvironmentFailure
criticalMissing.length > 0
```

Doctor raporu:

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

### Konfigürasyon Hatası

Koşul:

```text
exitCode=64
failureKind=ConfigurationFailure
```

Doctor raporu:

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

### Process / Protokol / Agent / Bilinmeyen Hata

Koşul:

```text
exitCode=3
failureKind in ProcessFailure | ProtocolFailure | AgentFailure | Unknown
```

Doctor raporu:

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

## Adapter Sözde Kodu

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

## Açık Sorular

1. OpenClaw bu probe'u `doctor --lint` içinde mi, `acpx doctor` içinde mi, yoksa ayrı bir plugin komutu olarak mı çağırmalı?
2. Node runtime -> Windows interop hatası nasıl aşılacak?
3. Probe sonucunda uyarı varken doctor exit code ne olmalı?
   - `doctor --lint --severity-min warning` için finding varsa exit 1.
   - Runtime doctor raporu için `ok=true` kalabilir.
4. `installCommand` sadece sistem paketleri için mi kullanılmalı?
   - Örnek: `sudo apt install ripgrep`
   - Windows/WSL ve distro farkları nedeniyle varsayılan olarak `fixHint` daha güvenli.
