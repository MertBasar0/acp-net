# Spike 011 Sonuc Raporu: OpenClaw Plugin/Command Integration Probe

Tarih: 2026-06-09

## Ozet

Spike kismen basarili ve urun acisindan yararli bir sinir bulgusu verdi.

OpenClaw kaynak agacina dogrudan entegrasyon yapilmadi. Bunun yerine Acp.Net workspace icinde iki probe hazirlandi:

1. Dogrulanan C# external command probe.
2. OpenClaw/Node wrapper fikrini temsil eden Node probe.

## OpenClaw Tarafinda Okunan Baglam

OpenClaw icinde zaten `extensions/acpx` eklentisi var.

Ilgili gozlemler:

- `extensions/acpx/openclaw.plugin.json` ACPX Runtime eklentisini tanimliyor.
- `extensions/acpx/index.ts` startup'ta ACP runtime backend service kaydediyor.
- `extensions/acpx/register.runtime.ts` runtime backend'i lazy register ediyor.
- `extensions/acpx/src/process-lease.ts` ACPX wrapper process'leri icin lease/state contract'i tutuyor.

Bu nedenle Spike 011'de OpenClaw core'a dogrudan mudahale edilmedi. Daha dogru kucuk adim, Acp.Net'in OpenClaw tarafindan external command/tool olarak cagrilabilecek sonuc contract'ini kanitlamak oldu.

## Eklenen Probe'lar

### C# Probe

Konum:

`src/samples/openclaw-acpnet-probe/`

Bu probe:

1. fake ACP agent olusturur,
2. `AcpProcessRunner` ile agent'i calistirir,
3. `python3` icin critical preflight, `rg` icin optional preflight yapar,
4. ACP initialize/session/prompt akisini kosar,
5. transcript ve run artifact uretir,
6. stdout'a tek OpenClaw-oriented JSON sonuc basar.

Bu probe dogrulandi.

Komut:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\openclaw-acpnet-probe\openclaw-acpnet-probe.csproj'
```

Sonuc ozeti:

```json
{
  "kind": "openclaw.acpnet.probe.result",
  "ok": true,
  "result": "completed",
  "failureKind": "None",
  "sessionId": "fake-session-1",
  "stopReason": "EndTurn",
  "usesWsl": true,
  "preflight": {
    "criticalMissing": [],
    "warnings": [
      {
        "name": "rg",
        "found": false,
        "missingPolicy": "Warn"
      }
    ]
  }
}
```

Son artifact:

`artifacts/openclaw-probe/20260608-212704/probe-run.json`

Son transcript:

`artifacts/openclaw-probe/20260608-212704/probe-transcript.ndjson`

### Node Probe

Konum:

`src/openclaw-probe/openclaw-acpnet-probe.mjs`

Bu probe OpenClaw'in Node/TypeScript command wrapper tarafini temsil etmek icin yazildi:

1. `dotnet run` ile Acp.Net sample'ini baslatir,
2. stdout JSON sonucunu parse eder,
3. run artifact'i okur,
4. normalize OpenClaw sonuc JSON'u basar.

Bu ortamda end-to-end dogrulanamadi.

Sebep:

Node child process icinden Windows interop komutlari (`dotnet.exe`, `powershell.exe`) su hata ile dusuyor:

```text
WSL ERROR: UtilBindVsockAnyPort:307: socket failed 1
```

Ayni `dotnet run` komutu shell'den dogrudan calisiyor. Bu nedenle hata Acp.Net runner davranisindan cok, Codex sandbox altinda Node child process -> Windows interop siniri gibi gorunuyor.

Bu bulgu OpenClaw entegrasyonu icin onemli:

> OpenClaw Node runtime'i WSL icinden Windows interop executable cagiracaksa, bu yol kendi runtime/sandbox kosullarinda ayrica dogrulanmali.

## Test ve Paket Dogrulamasi

Ana test suite:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
```

Pack komutlari exit code 0 ile tamamlandi:

- `Acp.Net.Process`
- `Acp.Net.Testing`

## Karar Etkisi

Spike 011, Acp.Net'in OpenClaw icin degerini daha somut hale getirdi:

- OpenClaw zaten ACP runtime/backend kavramina sahip.
- Acp.Net bu backend'in yerini almak zorunda degil.
- Acp.Net, external command veya future plugin bridge olarak daha guclu bir pozisyona sahip:
  - deterministic preflight,
  - failureKind ayrimi,
  - transcript,
  - run artifact,
  - WSL/path/runtime evidence.

## Sonraki Mantikli Is

Spike 012 onerisi:

**OpenClaw ACPX Contract Comparison**

Amac:

1. `extensions/acpx` runtime contract'ini daha yakindan incele.
2. Acp.Net run artifact/failure contract ile ACPX process lease/runtime event contract'ini yan yana koy.
3. Acp.Net'in OpenClaw'a hangi sekilde en az tekrar ve en cok degerle entegre olabilecegine karar ver:
   - external command,
   - acpx runtime backend adapter,
   - diagnostic probe only,
   - test harness package.

Bu karar verilmeden OpenClaw core'a kod eklenmemeli.

