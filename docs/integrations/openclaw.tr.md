# OpenClaw Entegrasyonu (Referans)

> 🇬🇧 English version: [openclaw.md](openclaw.md)

Bu, bir referans entegrasyon rehberidir; OpenClaw'da yayınlanmış bir özellik
değildir. OpenClaw'ın Acp.Net'i bir **harici tanılama komutu** olarak nasıl
çağırabileceğini ve sonucu doctor/lint health check'leri üzerinden nasıl
gösterebileceğini anlatır. Arkasındaki strateji
[OPENCLAW_INTEGRATION_STRATEGY.tr.md](../OPENCLAW_INTEGRATION_STRATEGY.tr.md)'de;
kararlar [ADR-0002](../decisions/ADR-0002-independent-package-openclaw-reference.tr.md)
ve [ADR-0003](../decisions/ADR-0003-diagnostics-remains-tooling.tr.md)'te.

## Ne olduğu — ve ne olmadığı

- **Olduğu:** opt-in, üçüncü taraf bir tanılama. OpenClaw bir komut çalıştırır,
  tek bir JSON sonucu okur ve onu health finding'lere eşler. Acp.Net, ACPX'in
  kendiliğinden göstermediği environment / preflight / hata kanıtını sağlar.
- **Olmadığı:** bir ACPX replacement'ı. OpenClaw, ACP session/turn/event
  runtime'ına ve process-lease temizliğine `extensions/acpx` içinde zaten sahip.
  Acp.Net, OpenClaw turn'lerini çalıştırmaz ve OpenClaw core'a zorunlu .NET
  bağımlılığı eklemez.

Acp.Net'in bir health check'e getirdiği ayrım, ACPX'in kendiliğinden çizmediği
ayrımdır:

> environment hatası ≠ agent hatası ≠ process hatası ≠ protokol hatası.

## Entegrasyon şekli

```
OpenClaw doctor / health check
        │  harici komutu çalıştır
        ▼
openclaw-acpnet-probe  ──►  stdout'a tek JSON sonucu  (+ exit code)
        │
        ▼
adapter sonucu eşler ──►  AcpRuntimeDoctorReport / HealthFinding[]
```

Probe, [`src/samples/openclaw-acpnet-probe/`](../../src/samples/openclaw-acpnet-probe/)
altındaki stabilize edilmiş sample'dır. Varsayılan olarak deterministik bir sahte
ACP agent çalıştırır (LLM yok, model kotası yok); böylece bir health check onu her
`doctor` çağrısında maliyetsiz çalıştırabilir.

## Tanılama komutu sözleşmesi

Varsayılan probe (sahte agent):

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj
```

Bunun yerine gerçek bir ACP uyumlu agent'a yöneltmek:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj -- \
  --agent gemini \
  --command /path/to/gemini \
  --arg --acp \
  --required-tool git \
  --optional-tool rg
```

Sözleşme garantileri:

- **stdout** tam olarak tek bir JSON sonucu taşır (bir
  `openclaw.acpnet.probe.result` nesnesi). stdout'a başka hiçbir şey yazılmaz.
- **stderr** yalnızca tanılama/yardım metnidir.
- **exit code'lar** araç çağıranlar için stabildir:

  | Kod | Anlamı |
  | --- | --- |
  | `0` | `ok=true` |
  | `2` | environment / preflight hatası |
  | `3` | runtime / protokol / agent / bilinmeyen hata |
  | `64` | geçersiz CLI konfigürasyonu |

Başarılı bir sonuç şöyle görünür (bkz.
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

## Sonucu OpenClaw yüzeylerine eşlemek

Tam eşleme — `AcpRuntimeDoctorReport` ve `HealthFinding[]`'e —
[contracts/openclaw-doctor-adapter-draft.tr.md](../contracts/openclaw-doctor-adapter-draft.tr.md)'de
tanımlı; makine tarafından kontrol edilebilir senaryolar
[contracts/openclaw-doctor-mapping.scenarios.json](../contracts/openclaw-doctor-mapping.scenarios.json)'da
ve çalıştırılabilir adapter taslağı + doğrulayıcısı
[`src/openclaw-probe/`](../../src/openclaw-probe/)'da. Doğrulayıcıyı çalıştırın:

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
# doctor adapter scenarios ok (4)
```

Tek tabloda eşleme:

| Probe sonucu | Runtime doctor | Lint finding |
| --- | --- | --- |
| `ok`, uyarı yok | `ok: true` | yok |
| `ok`, opsiyonel araç eksik | `ok: true`, `ACPNET_PREFLIGHT_WARNING` | `warning` |
| environment hatası (exit 2) | `ok: false`, `ACPNET_ENVIRONMENT_FAILURE` | `error` |
| konfigürasyon hatası (exit 64) | `ok: false`, `ACPNET_PROBE_CONFIG_INVALID` | `error` |
| process/protokol/agent hatası (exit 3) | `ok: false`, `ACPNET_PROBE_FAILED` | `error` |

Eksik bir **opsiyonel** araç runtime doctor'ı yeşil tutar (`ok: true`) ama lint
yüzeyinde yine de bir `warning` finding üretmelidir; eksik bir **kritik** araç ise
ikisinde de error'dır.

## Minimal bir OpenClaw health check (taslak)

```ts
import { mapProbeResultToDoctorReport } from "./acpnet-doctor-adapter";

async function acpnetHealthCheck(): Promise<AcpRuntimeDoctorReport> {
  const { stdout, exitCode } = await runCommand("openclaw-acpnet-probe", []);
  const result = JSON.parse(stdout); // tam olarak tek JSON nesnesi
  return mapProbeResultToDoctorReport(result, exitCode);
}
```

Adapter fonksiyonları (`mapProbeResultToDoctorReport`,
`mapProbeResultToHealthFindings`)
[`src/openclaw-probe/doctor-adapter-draft.mjs`](../../src/openclaw-probe/doctor-adapter-draft.mjs)
içinde taslak olarak zaten mevcut.

## Runtime sınırı uyarısı

OpenClaw'ın Node runtime'ı, WSL içinden Windows interop'a geçen bir komut
çalıştırdığında, o yolun OpenClaw'ın **gerçek** runtime'ında doğrulanması gerekir.
Sandbox'lı bir probe, Node-child-process → Windows-interop sınırında
`UtilBindVsockAnyPort` hatasına takıldı; aynı komut shell'den doğrudan sorunsuz
çalıştı. Probe'u OpenClaw'a bağlamadan önce nasıl çalıştırılacağına karar verin:

- host tarafı komut olarak (Windows host process'i), veya
- WSL-native bir `dotnet` kurulumu, veya
- sandbox dışında onaylı bir harici komut.

Gerçek bir entegrasyon için asıl açık soru bu sınırdır, eşleme değil.

## Olgunluk ve maintainer-dostu yol

Bu, komut sözleşmesi daha fazla kullanım kanıtı toplayana kadar referans/sample
olarak kalır (ADR-0003 uyarınca). Bir gün bir OpenClaw PR'ı önerilirse küçük ve
entegrasyon-noktası odaklı olmalı: geniş kapsamlı runtime yeniden yazımı yok,
core'da zorunlu .NET bağımlılığı yok, ACPX replacement yok, tüm davranış opt-in
konfigürasyon veya plugin/sample yolunun arkasında.

## Artifact dizini

- Probe sample: [`src/samples/openclaw-acpnet-probe/`](../../src/samples/openclaw-acpnet-probe/)
- Node komut-wrapper'ı + adapter taslağı + doğrulayıcı: [`src/openclaw-probe/`](../../src/openclaw-probe/)
- Eşleme kontratı: [contracts/openclaw-doctor-adapter-draft.tr.md](../contracts/openclaw-doctor-adapter-draft.tr.md)
- Eşleme senaryoları: [contracts/openclaw-doctor-mapping.scenarios.json](../contracts/openclaw-doctor-mapping.scenarios.json)
- Sonuç/artifact örnekleri: [contracts/openclaw-acpnet-probe-result.example.json](../contracts/openclaw-acpnet-probe-result.example.json), [contracts/acpnet-run-artifact.example.json](../contracts/acpnet-run-artifact.example.json)
- Strateji & kararlar: [OPENCLAW_INTEGRATION_STRATEGY.tr.md](../OPENCLAW_INTEGRATION_STRATEGY.tr.md), [ADR-0002](../decisions/ADR-0002-independent-package-openclaw-reference.tr.md), [ADR-0003](../decisions/ADR-0003-diagnostics-remains-tooling.tr.md)
