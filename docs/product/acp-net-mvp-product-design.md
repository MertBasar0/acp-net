# Acp.Net MVP Urun Tasarimi

Tarih: 2026-06-07

## Urun Iddiasi

Acp.Net, .NET icin yeni bir tam ACP protokol SDK'si olarak konumlanmayacak.

Acp.Net'in ilk urun iddiasi:

> .NET uygulamalarinda ACP agent process'lerini guvenilir sekilde baslatmak, yonetmek, debug etmek ve test etmek icin process/runtime/testing katmani.

Bu konum, mevcut `AgentClientProtocol` paketinin yerine gecmek yerine onu tamamlamayi hedefler.

## Hedef Kullanici

Ilk hedef kullanici:

- ACP agent'i .NET uygulamasindan baslatmak isteyen gelistirici.
- Windows uzerinde WSL/Linux agent calistiran gelistirici.
- Stdio tabanli agent entegrasyonunu test etmek isteyen SDK/IDE/plugin gelistiricisi.
- CI'da agent process lifecycle testlerini deterministik yapmak isteyen ekip.

Hedef disi:

- ACP protokol specification'ini bastan modellemek isteyenler.
- UI/editor entegrasyonu arayanlar.
- Training Factory veya RL urunlesmesi.
- Genel purpose process manager arayanlar.

## MVP Paketleri

Ilk paketleme iki parcaya ayrilmali.

### Acp.Net.Process

Sorumluluk:

- ACP agent process baslatma.
- stdin/stdout/stderr yonetimi.
- Windows/WSL runtime bridge.
- path normalization.
- timeout ve shutdown stratejisi.
- raw transcript yakalama.

Bu paket protocol schema'larini sahiplenmez. Mevcut `AgentClientProtocol` paketiyle beraber calisabilmelidir.

### Acp.Net.Testing

Sorumluluk:

- Fake ACP agent/server.
- Transcript assertion helper.
- Golden transcript testleri.
- Timeout/cancel/process-exit senaryolari icin test fixture.

Bu paket test odakli olmalidir; production runtime bagimliligi minimum tutulmalidir.

## MVP'ye Girecekler

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpRuntimeResolver`
- `AcpPathMapper`
- `AcpTranscriptRecorder`
- `AcpShutdownPolicy`
- `FakeAcpAgent`
- transcript assertion helper

## MVP'ye Girmeyecekler

- Tam ACP schema modeli.
- `InitializeRequest`, `PromptRequest` gibi typed protocol surface'i.
- UI/dashboard.
- Provider marketplace.
- Training Factory entegrasyonu.
- PX4/Gazebo/drone workflow.
- Long-running daemon/service yonetimi.

## Basit API Taslagi

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/mertb/agent.py"],
    WorkingDirectory = "/home/mertb/project",
    Runtime = AcpRuntime.Auto,
    TranscriptPath = "agent-transcript.ndjson",
    Shutdown = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromSeconds(2))
});

await using var session = await runner.StartAsync();

using var connection = new ClientSideConnection(
    _ => client,
    session.Stdout,
    session.Stdin);

connection.Open();

var init = await connection.InitializeAsync(...);
```

Windows host + WSL agent icin:

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/mertb/agent.py"],
    Runtime = AcpRuntime.Wsl,
    WslDistribution = "Ubuntu"
});
```

Test helper icin:

```csharp
await using var fake = await FakeAcpAgent.StartAsync(new FakeAcpAgentOptions
{
    Script = FakeAcpScript.Default()
        .OnInitialize()
        .OnNewSession("test-session")
        .OnPrompt(stream: ["hello", " world"], stopReason: "end_turn")
});

var runner = AcpProcessRunner.ForExistingProcess(fake.Process);
```

## Basari Kriterleri

MVP degerli sayilmasi icin:

1. `AgentClientProtocol` ile beraber calisan minimal sample sunar.
2. Windows `dotnet.exe` -> WSL `python3` agent senaryosunu tek ayarla calistirir.
3. stdin/stdout/stderr akisini kayipsiz transcript olarak kaydeder.
4. Process kapanmazsa once graceful shutdown, sonra hard kill uygular.
5. CI'da fake ACP agent ile timeout/cancel/streaming testleri deterministik kosar.
6. Entegrasyon yapan uygulama kodundaki process glue miktarini belirgin azaltir.

## Urun Degeri

Deger protokol typing'de degil, entegrasyon maliyetini dusurmede.

Mevcut paketin iyi oldugu alan:

- ACP method isimleri.
- schema tipleri.
- request/notification dispatch.

Acp.Net'in iyi olmasi gereken alan:

- gercek process davranisi.
- Windows/WSL gercekligi.
- debug edilebilirlik.
- test edilebilirlik.

## Riskler

- Kapsam tekrar tam SDK'ya kayabilir.
- Mevcut `AgentClientProtocol` paketi bu helper'lari eklerse fark azalir.
- Windows/WSL senaryolari makineye ve distro'ya gore degisebilir.
- Test helper ile production runner ayni pakete koyulursa API sisebilir.

## Mitigasyon

- Protokol schema yazilmamali; mevcut paketle entegrasyon tercih edilmeli.
- Process ve Testing paketleri ayrilmali.
- Ilk MVP sadece stdio ACP agent senaryosuna odaklanmali.
- Her feature bir failing integration testten gelmeli.

## 2026-06-07 API Hardening Notu

Paket id'leri `Acp.Net.Process` ve `Acp.Net.Testing` olarak korundu. C# namespace'leri ise `AcpNet.Process` ve `AcpNet.Testing` olarak sadeleştirildi. Bunun nedeni `Acp.Net.Process` namespace'inin `System.Diagnostics.Process` ile gereksiz isim carpismasi yaratmasidir.

Ilk alpha public yuzeyi su tipe daraltildi:

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpProcessSession`
- `AcpRuntime`
- `AcpShutdownPolicy`
- `AcpTranscriptRecorder`
- `FakeAcpAgentScript`
- `AcpTranscriptAssert`

## 2026-06-07 Gemini Dogfood Notu

Gercek Gemini CLI ACP agent ile dogfood basarili oldu. Bu dogfood `AcpProcessSession.ToAgentPath(...)` ihtiyacini ortaya cikardi: agent WSL icinde calistiginda sadece process start path'i degil, ACP payload icindeki `cwd` gibi path alanlari da WSL path'e donusturulmeli.

Ek urunlestirme gereksinimi:

- WSL non-login process ortaminda PATH farklari gorulebiliyor. Gemini stderr'da `Ripgrep is not available. Falling back to GrepTool.` uyarisi goruldu. Runner ileride environment/PATH veya login-shell stratejisi sunmali.

## 2026-06-07 Runtime Environment Shaping Notu

Bu gereksinim icin ilk API eklendi:

- `AcpProcessOptions.Environment`
- `AcpProcessOptions.AdditionalPathEntries`
- `AcpProcessOptions.RequiredExecutables`

Runner artik agent baslamadan once required executable preflight yapip transcript'e `preflight.tool.found` veya `preflight.tool.missing` event'i yaziyor.

Gemini dogfood'da `rg` eksikligi agent calismadan once `preflight.tool.missing` olarak yakalandi; `git` ve `node` bulundu.

## 2026-06-09 OpenClaw Runtime Substrate Notu

Spike 008-010 sonucunda Acp.Net'in OpenClaw icindeki muhtemel rolu daha netlesti.

Acp.Net, OpenClaw icin core orchestrator degil; ACP-compatible agent/subagent process'lerini guvenilir ve denetlenebilir sekilde calistiran runtime substrate olarak konumlanmali.

Eklenen urun kabiliyetleri:

- per-tool preflight policy: `Warn` veya `Throw`
- fail-fast environment failure
- `AcpPreflightException`
- `AcpRunFailureKind`
- makine-okunabilir `AcpRunArtifact`
- `RunArtifactPath`
- OpenClaw-style deterministic subagent runner sample

Bu karar su ayrimi urunlesme acisindan merkezi hale getirir:

> Agent basarisizligi ile environment basarisizligi ayni sey degildir.

OpenClaw gibi bir orkestrator, agent sonucunu degerlendirmeden once runtime environment'in saglamligini bilmelidir. Acp.Net'in degeri bu kanit katmanini saglamasidir.

## 2026-06-09 Spike 011 OpenClaw Probe Notu

OpenClaw kaynak agacinda `extensions/acpx` zaten ACP runtime/backend ve process lease yonetimi sagliyor. Bu nedenle Acp.Net'i OpenClaw'a dogrudan ikinci bir runtime olarak eklemek erken.

Spike 011'de iki entegrasyon sekli denendi:

- C# external command probe basarili oldu.
- Node wrapper probe, Codex sandbox altinda Node child process -> Windows interop sinirinda `UtilBindVsockAnyPort` hatasina takildi.

Bu bulgu Acp.Net'in degerini azaltmiyor; tersine OpenClaw entegrasyonunda runtime boundary'nin ne kadar kritik oldugunu tekrar gosteriyor.

Bir sonraki karar, Acp.Net'in OpenClaw'da hangi rolde daha az tekrar ve daha cok deger urettigidir:

- external diagnostic command,
- ACPX backend adapter helper,
- test harness package,
- veya sadece run artifact/failure contract saglayicisi.

## 2026-06-09 Spike 012 ACPX Contract Karari

OpenClaw `extensions/acpx` ve `packages/acp-core` contract'leri incelendi.

Karar:

> Acp.Net, ACPX runtime backend replacement olmamali.

Gerekce:

- ACPX zaten OpenClaw'in session/turn/event runtime contract'ini uyguluyor.
- ACPX process lease ve cleanup state'ini OpenClaw plugin state sistemiyle entegre ediyor.
- Acp.Net'in en guclu yani runtime event streaming degil; process evidence, preflight, failure classification ve test harness.

Kisa vadeli urun yolu:

1. Acp.Net diagnostic command contract'ini stabilize et.
2. OpenClaw bunu doctor/plugin command olarak cagirabilsin.
3. Artifact ve transcript uzerinden environment failure ile agent failure ayrimini gostersin.
4. Derin ACPX entegrasyonuna ancak bu command contract kullanisli olduktan sonra girilsin.

## 2026-06-09 Spike 013 Diagnostic Command Notu

`openclaw-acpnet-probe` artik daha stabil bir CLI contract sagliyor.

Desteklenen kararlar:

- stdout tek JSON result icin ayrildi.
- exit code sozlesmesi netlesti: `0`, `2`, `3`, `64`.
- fake-agent default korunarak model kotasi harcamayan dogrulama yolu saglandi.
- real ACP-compatible command icin `--command` ve repeatable `--arg` eklendi.
- tool policy argumanlari `--required-tool` ve `--optional-tool` olarak disaridan verilebilir hale geldi.

Bu, OpenClaw core'a dogrudan kod eklemeden once gerekli ara urunlesme adimidir.

## 2026-06-09 Spike 014 Doctor Adapter Notu

Diagnostic command sonucunun OpenClaw doctor/lint yuzeylerine mapping contract'i hazirlandi.

Karar:

- `AcpRuntimeDoctorReport` runtime doctor icin uygun yuzey.
- `HealthFinding[]` doctor lint icin uygun yuzey.
- Optional tool eksikligi runtime doctor icin `ok=true` kalabilir ama lint yuzeyinde `warning` finding olmalidir.
- Critical environment failure doctor ve lint icin error olmalidir.

Bu, Acp.Net'in OpenClaw'a ilk entegrasyonunun doctor/lint evidence seklinde olmasi gerektigini guclendirir.
