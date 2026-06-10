# Spike 014 Sonuc Raporu: OpenClaw Doctor Adapter Draft

Tarih: 2026-06-09

## Ozet

Spike basarili.

Acp.Net diagnostic probe sonucunun OpenClaw doctor/lint yuzeylerine nasil map edilecegi yazili contract ve executable draft olarak hazirlandi.

OpenClaw core'a kod eklenmedi.

## Incelenen OpenClaw Yuzeyleri

Doctor CLI dokumani:

`docs/cli/doctor.md`

Structured health finding contract:

`src/flows/health-checks.ts`

Runtime doctor report contract:

`packages/acp-core/src/runtime/types.ts`

Ilgili hedef modeller:

```ts
type AcpRuntimeDoctorReport = {
  ok: boolean;
  code?: string;
  message: string;
  installCommand?: string;
  details?: string[];
};
```

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

## Eklenen Contract Dokumani

Ana contract:

`docs/contracts/openclaw-doctor-adapter-draft.md`

Senaryo fixture:

`docs/contracts/openclaw-doctor-mapping.scenarios.json`

Bu contract su mappingleri tanimliyor:

- clean success -> doctor report ok, finding yok
- optional tool warning -> doctor report ok ama warning code/details, lint warning finding
- environment failure -> doctor report error, lint error finding
- configuration failure -> doctor report error, lint error finding
- process/protocol/agent/unknown failure -> generic probe failed report/finding

## Eklenen Executable Draft

Adapter draft:

`src/openclaw-probe/doctor-adapter-draft.mjs`

Verifier:

`src/openclaw-probe/verify-doctor-adapter-draft.mjs`

Verifier, fixture dosyasindaki 4 senaryo icin:

- `mapProbeResultToDoctorReport`
- `mapProbeResultToHealthFindings`

fonksiyonlarinin beklenen output ile birebir uyustugunu kontrol eder.

## Dogrulama

Adapter verifier:

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

Sonuc:

```text
doctor adapter scenarios ok (4)
```

Ana test suite:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
```

Pack:

```text
Acp.Net.Process: ok
Acp.Net.Testing: ok
```

## Karar Etkisi

Bu spike, Acp.Net'in OpenClaw icindeki kisa vadeli rolunu daha da netlestirdi:

> Acp.Net, once OpenClaw doctor/lint tarafina diagnostic evidence veren external probe olarak entegre edilmeli.

Bu yol:

- ACPX runtime replacement degil,
- OpenClaw core'a minimum riskle girer,
- environment failure ile agent/runtime failure ayrimini gorunur yapar,
- mevcut doctor/lint semantigine uyum saglar.

## Sonraki Mantikli Is

Spike 015 onerisi:

**OpenClaw Doctor Adapter Implementation Probe**

Hedef:

1. OpenClaw kaynak agacinda gercek `HealthCheck` adapter'inin minimum kod degisikligini tasarla.
2. Adapter'in `detect()` fonksiyonu Acp.Net diagnostic command'i nasil calistiracak belirle.
3. Node child process -> Windows interop problemi icin calistirma stratejisini sec:
   - Windows host command,
   - WSL-native dotnet install,
   - approved external command,
   - veya sadece docs/manual doctor step.
4. Kod yazilacaksa once test-first ve fake JSON fixture ile yaz.

