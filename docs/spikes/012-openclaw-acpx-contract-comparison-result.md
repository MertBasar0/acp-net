# Spike 012 Sonuc Raporu: OpenClaw ACPX Contract Comparison

Tarih: 2026-06-09

## Ozet

Spike basarili.

OpenClaw icindeki `extensions/acpx` runtime contract'i ile Acp.Net'in run artifact/failure contract'i karsilastirildi.

Net karar:

> Acp.Net, OpenClaw ACPX runtime backend'inin yerine gecmemeli. En degerli rol, ACPX'in altinda veya yaninda calisan diagnostic/process evidence/test harness katmani.

## Incelenen OpenClaw Contract'leri

### ACP Runtime Contract

Ana kaynak:

`packages/acp-core/src/runtime/types.ts`

OpenClaw ACP runtime modeli sunlara odaklaniyor:

- `ensureSession(input): Promise<AcpRuntimeHandle>`
- `startTurn(input): AcpRuntimeTurn`
- `runTurn(input): AsyncIterable<AcpRuntimeEvent>`
- `cancel(handle)`
- `close(handle)`
- `doctor()`
- `getStatus()`
- `getCapabilities()`

Runtime event modeli:

- `text_delta`
- `status`
- `tool_call`
- `done`
- `error`

Turn result modeli:

- `completed`
- `cancelled`
- `failed`

### ACPX Runtime Adapter

Ana kaynak:

`extensions/acpx/src/runtime.ts`

ACPX adapter'in sorumluluklari:

- upstream `acpx/runtime` uzerinden ACP session yonetmek,
- OpenClaw session metadata eklemek,
- model/thinking override uygulamak,
- turn timeout sahipligini OpenClaw'a almak,
- Codex wrapper stderr tail'i ile generic internal error'lari zenginlestirmek,
- MCP bridge-safe delegate secmek,
- close/cancel akisinda cleanup calistirmak.

### Process Lease Contract

Ana kaynak:

`extensions/acpx/src/process-lease.ts`

Lease modeli:

```ts
type AcpxProcessLease = {
  leaseId: string;
  gatewayInstanceId: string;
  sessionKey: string;
  wrapperRoot: string;
  wrapperPath: string;
  rootPid: number;
  processGroupId?: number;
  commandHash: string;
  startedAt: number;
  state: "open" | "closing" | "closed" | "lost";
};
```

Bu contract, OpenClaw'in ACPX wrapper process'lerini sonradan bulup kapatabilmesine odaklaniyor.

## Acp.Net Contract'i

Ana kaynak:

`src/acp-net/Acp.Net.Process/AcpRunArtifact.cs`

Acp.Net run artifact modeli:

```csharp
public sealed record AcpRunArtifact(
    string RunId,
    string AgentName,
    string? WorkingDirectory,
    bool UsesWsl,
    string ResolvedCommandLine,
    string Result,
    AcpRunFailureKind FailureKind,
    string? FailureMessage,
    string? TranscriptPath,
    IReadOnlyList<AcpExecutablePreflightResult> Preflight,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt);
```

Failure modeli:

```csharp
public enum AcpRunFailureKind
{
    None,
    EnvironmentFailure,
    ProcessFailure,
    ProtocolFailure,
    AgentFailure,
    Unknown
}
```

Acp.Net su sorulara cevap veriyor:

- agent hangi resolved command line ile baslatildi?
- WSL kullanildi mi?
- hangi working directory kullanildi?
- hangi executable bulundu veya eksikti?
- eksik tool critical miydi warning miydi?
- transcript nerede?
- failure environment mi process mi protocol mu?

## Yan Yana Degerlendirme

| Alan | ACPX / OpenClaw | Acp.Net |
| --- | --- | --- |
| Ana odak | session/turn runtime | process evidence/runtime diagnostics |
| API merkezi | `ensureSession`, `startTurn`, event stream | `AcpProcessRunner`, transcript, run artifact |
| Failure modeli | ACP runtime error code + turn result | `AcpRunFailureKind` + preflight result |
| Process cleanup | lease store + process reaper | graceful shutdown + kill policy |
| Tool preflight | runtime config/probe tarafinda dolayli | first-class `RequiredTools` |
| WSL/path evidence | adapter/wrapper davranisi icinde | first-class `UsesWsl`, `ToAgentPath`, resolved command |
| Transcript | OpenClaw runtime stream/log surfaces | raw stdio + event NDJSON |
| En uygun rol | orchestrator/runtime backend | diagnostic substrate/test harness |

## Kritik Sonuc

ACPX ve Acp.Net ayni problemi ayni seviyede cozmuuyor.

ACPX'in problemi:

> OpenClaw bir ACP session'i nasil acar, turn nasil baslatir, eventleri nasil stream eder, session'i nasil kapatir?

Acp.Net'in problemi:

> O ACP agent process'i gercekte hangi ortamda, hangi path/tool/env ile calisti ve failure nereden geldi?

Bu nedenle Acp.Net'i ACPX yerine koymak tekrar ve risk yaratir. Ama Acp.Net'in evidence/failure/preflight contract'i ACPX'in eksik gorunurluk alanini tamamlayabilir.

## Entegrasyon Secenekleri

### 1. External Diagnostic Command

OpenClaw bir command/tool olarak Acp.Net probe'u calistirir.

Artisi:

- en dusuk risk
- OpenClaw core'a az dokunur
- bugunku C# probe ile dogrulandi
- diagnostic/doctor akisi icin uygun

Eksisi:

- runtime turn path'ine gomulu degil
- surekli subagent orchestration icin ikinci adim gerekir

Karar: **kisa vadede en iyi secenek.**

### 2. ACPX Backend Adapter Helper

ACPX agent process baslatirken Acp.Net.Process benzeri preflight/artifact mantigini kullanir.

Artisi:

- en buyuk urun degeri burada olabilir
- agent failure ve environment failure ayrimi runtime'a dogrudan girer

Eksisi:

- OpenClaw TypeScript runtime ile .NET library boundary zor
- Node child process -> Windows interop siniri Spike 011'de sorun cikardi
- ACPX zaten process lease/reaper sahibi; tekrar riski var

Karar: **orta vadede arastirilmali, simdi erken.**

### 3. Test Harness Package

OpenClaw ACPX testleri, Acp.Net.Testing fake ACP agent/transcript fixture mantigindan yararlanir.

Artisi:

- gercek runtime'a dokunmadan deger uretir
- deterministic agent/process boundary testleri saglar

Eksisi:

- .NET test harness ile TypeScript test runner arasinda ek is gerekir

Karar: **iyi ikinci secenek.**

### 4. Full ACPX Replacement

Acp.Net, ACPX runtime backend'in yerine gecer.

Artisi:

- .NET tarafinda guclu process kontrolu

Eksisi:

- OpenClaw'in mevcut TS runtime/event/session yuzeyiyle buyuk tekrar
- cok yuksek entegrasyon maliyeti
- urun pozisyonuyla uyumsuz

Karar: **onerilmiyor.**

## Karar

Kisa vadeli karar:

> Acp.Net, OpenClaw icin external diagnostic command + run artifact/failure contract olarak ilerlemeli.

Orta vadeli karar:

> ACPX ile daha derin entegrasyon dusunulurse, once Node runtime -> Windows/WSL interop siniri ve ACPX process lease contract'i ile uyum kanitlanmali.

## Eklenen Contract Fixture'lari

Fixture dosyalari:

- `docs/contracts/acpnet-run-artifact.example.json`
- `docs/contracts/openclaw-acpnet-probe-result.example.json`

Bu dosyalar ileride OpenClaw plugin/command adapter testlerinde golden input olarak kullanilabilir.

## Sonraki Mantikli Is

Spike 013 onerisi:

**Acp.Net Diagnostic Command Stabilization**

Amac:

1. `openclaw-acpnet-probe` sample'ini daha stabil bir CLI contract'a donustur.
2. `--agent`, `--cwd`, `--required-tool`, `--optional-tool`, `--transcript`, `--artifact` parametreleri ekle.
3. stdout'u tek JSON result olarak garanti et.
4. stderr'i diagnostic-only tut.
5. OpenClaw tarafinin bunu `doctor` veya plugin command olarak cagirmasina uygun hale getir.

Bu, OpenClaw core'a dokunmadan urun degerini netlestiren en guvenli sonraki adimdir.

