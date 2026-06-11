# Acp.Net MVP Ürün Tasarımı

> 🇬🇧 English version: [acp-net-mvp-product-design.md](acp-net-mvp-product-design.md)

Tarih: 2026-06-07

## Ürün İddiası

Acp.Net, .NET için yeni bir tam ACP protokol SDK'sı olarak konumlanmayacak.

Acp.Net'in ilk ürün iddiası:

> .NET uygulamalarında ACP agent process'lerini güvenilir şekilde başlatmak, yönetmek, debug etmek ve test etmek için process/runtime/test katmanı.

Bu konum, mevcut `AgentClientProtocol` paketinin yerine geçmek yerine onu tamamlamayı hedefler.

## Hedef Kullanıcı

İlk hedef kullanıcılar:

- ACP agent'ı .NET uygulamasından başlatmak isteyen geliştirici.
- Windows üzerinde WSL/Linux agent çalıştıran geliştirici.
- Stdio tabanlı agent entegrasyonunu test etmek isteyen SDK/IDE/plugin geliştiricisi.
- CI'da agent process yaşam döngüsü testlerini deterministik yapmak isteyen ekip.

Hedef dışı:

- ACP protokol spesifikasyonunu baştan modellemek isteyenler.
- UI/editor entegrasyonu arayanlar.
- Genel amaçlı process manager arayanlar.

## MVP Paketleri

İlk paketleme iki parçaya ayrılıyor.

### Acp.Net.Process

Sorumluluklar:

- ACP agent process'ini başlatma.
- stdin/stdout/stderr yönetimi.
- Windows/WSL runtime köprüsü.
- Path normalizasyonu.
- Timeout ve kapatma stratejisi.
- Ham transcript yakalama.

Bu paket protokol şemalarını sahiplenmez. Mevcut `AgentClientProtocol` paketiyle birlikte çalışabilmelidir.

### Acp.Net.Testing

Sorumluluklar:

- Sahte ACP agent/server.
- Transcript doğrulama yardımcıları.
- Golden transcript testleri.
- Timeout/cancel/process-exit senaryoları için test fixture'ları.

Bu paket test odaklı kalmalıdır; üretim runtime bağımlılığı minimumda tutulmalıdır.

## MVP'ye Girecekler

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpRuntimeResolver`
- `AcpPathMapper`
- `AcpTranscriptRecorder`
- `AcpShutdownPolicy`
- `FakeAcpAgent`
- transcript doğrulama yardımcıları

## MVP'ye Girmeyecekler

- Tam ACP şema modeli.
- `InitializeRequest`, `PromptRequest` gibi typed protokol yüzeyi.
- UI/dashboard.
- Sağlayıcı pazaryeri.
- Uzun süre çalışan daemon/servis yönetimi.

## Basit API Taslağı

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/<kullanıcı>/agent.py"],
    WorkingDirectory = "/home/<kullanıcı>/project",
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

Windows host + WSL agent için:

```csharp
var runner = new AcpProcessRunner(new AcpProcessOptions
{
    Command = "python3",
    Arguments = ["/home/<kullanıcı>/agent.py"],
    Runtime = AcpRuntime.Wsl,
    WslDistribution = "Ubuntu"
});
```

Test yardımcısı için:

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

## Başarı Kriterleri

MVP'nin değerli sayılması için:

1. `AgentClientProtocol` ile birlikte çalışan minimal bir sample sunar.
2. Windows `dotnet.exe` -> WSL `python3` agent senaryosunu tek ayarla çalıştırır.
3. stdin/stdout/stderr akışını kayıpsız transcript olarak kaydeder.
4. Process kapanmazsa önce nazik kapatma, sonra zorla sonlandırma uygular.
5. CI'da sahte ACP agent ile timeout/cancel/streaming testleri deterministik koşar.
6. Entegrasyon yapan uygulama kodundaki process glue miktarını belirgin azaltır.

## Ürün Değeri

Değer protokol typing'de değil, entegrasyon maliyetini düşürmede.

Mevcut paketin iyi olduğu alan:

- ACP metod isimleri.
- Şema tipleri.
- Request/notification dispatch.

Acp.Net'in iyi olması gereken alan:

- Gerçek process davranışı.
- Windows/WSL gerçekliği.
- Debug edilebilirlik.
- Test edilebilirlik.

## Riskler

- Kapsam tekrar tam SDK'ya kayabilir.
- Mevcut `AgentClientProtocol` paketi bu yardımcıları eklerse fark azalır.
- Windows/WSL senaryoları makineye ve distro'ya göre değişebilir.
- Test yardımcısı ile üretim runner'ı aynı pakete konursa API şişebilir.

## Önlemler

- Protokol şeması yazılmamalı; mevcut paketle entegrasyon tercih edilmeli.
- Process ve Testing paketleri ayrılmalı.
- İlk MVP sadece stdio ACP agent senaryosuna odaklanmalı.
- Her özellik bir başarısız entegrasyon testinden gelmeli.

## Tasarım Notları Günlüğü

Aşağıdaki notlar, tasarımın geliştirme sırasında nasıl evrildiğini kaydeder. Bunların arkasındaki ayrıntılı spike oturum raporları, depo kökündeki, git tarafından takip edilmeyen `notes/` klasöründe tutulmaktadır (uzak depoya gönderilmez).

### 2026-06-07 API Sağlamlaştırma Notu

Paket id'leri `Acp.Net.Process` ve `Acp.Net.Testing` olarak korundu. C# namespace'leri ise `AcpNet.Process` ve `AcpNet.Testing` olarak sadeleştirildi; çünkü `Acp.Net.Process` namespace'i `System.Diagnostics.Process` ile gereksiz isim çarpışması yaratıyor.

İlk alpha public yüzeyi şu tiplere daraltıldı:

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpProcessSession`
- `AcpRuntime`
- `AcpShutdownPolicy`
- `AcpTranscriptRecorder`
- `FakeAcpAgentScript`
- `AcpTranscriptAssert`

### 2026-06-07 Gemini Dogfood Notu

Gerçek Gemini CLI ACP agent ile dogfood başarılı oldu. Bu dogfood `AcpProcessSession.ToAgentPath(...)` ihtiyacını ortaya çıkardı: agent WSL içinde çalıştığında sadece process başlatma path'i değil, ACP payload içindeki `cwd` gibi path alanları da WSL path'ine dönüştürülmeli.

Ek ürünleştirme gereksinimi:

- WSL non-login process ortamında PATH farkları görülebiliyor. Gemini stderr'ında `Ripgrep is not available. Falling back to GrepTool.` uyarısı görüldü. Runner ileride environment/PATH veya login-shell stratejisi sunmalı.

### 2026-06-07 Runtime Environment Şekillendirme Notu

Bu gereksinim için ilk API'ler eklendi:

- `AcpProcessOptions.Environment`
- `AcpProcessOptions.AdditionalPathEntries`
- `AcpProcessOptions.RequiredExecutables`

Runner artık agent başlamadan önce gerekli executable'lar için preflight yapıp transcript'e `preflight.tool.found` veya `preflight.tool.missing` olayı yazıyor.

Gemini dogfood'unda eksik `rg`, agent çalışmadan önce `preflight.tool.missing` olarak yakalandı; `git` ve `node` bulundu.

### 2026-06-09 OpenClaw Runtime Substrate Notu

Spike 008–010 sonucunda Acp.Net'in OpenClaw içindeki muhtemel rolü netleşti.

Acp.Net, OpenClaw için core orchestrator değil; ACP uyumlu agent/subagent process'lerini güvenilir ve denetlenebilir şekilde çalıştıran runtime substrate olarak konumlanmalı.

Eklenen ürün kabiliyetleri:

- araç başına preflight politikası: `Warn` veya `Throw`
- hızlı-başarısız (fail-fast) environment hatası
- `AcpPreflightException`
- `AcpRunFailureKind`
- makine tarafından okunabilir `AcpRunArtifact`
- `RunArtifactPath`
- OpenClaw tarzı deterministik subagent runner sample'ı

Bu karar şu ayrımı ürünleşme açısından merkezî hale getirir:

> Agent başarısızlığı ile environment başarısızlığı aynı şey değildir.

OpenClaw gibi bir orkestratör, agent sonucunu değerlendirmeden önce runtime environment'ın sağlamlığını bilmelidir. Acp.Net'in değeri bu kanıt katmanını sağlamasıdır.

### 2026-06-09 Spike 011 OpenClaw Probe Notu

OpenClaw kaynak ağacında `extensions/acpx` zaten ACP runtime/backend ve process lease yönetimi sağlıyor. Bu nedenle Acp.Net'i OpenClaw'a ikinci bir runtime olarak eklemek erken.

Spike 011'de iki entegrasyon şekli denendi:

- C# harici komut probe'u başarılı oldu.
- Node wrapper probe'u, kısıtlı bir çalıştırma ortamında Node child process -> Windows interop sınırında `UtilBindVsockAnyPort` hatasına takıldı.

Bu bulgu Acp.Net'in değerini azaltmıyor; tersine OpenClaw entegrasyonunda runtime sınırının ne kadar kritik olduğunu tekrar gösteriyor.

### 2026-06-09 Spike 012 ACPX Kontrat Kararı

OpenClaw `extensions/acpx` ve `packages/acp-core` kontratları incelendi.

Karar:

> Acp.Net, ACPX runtime backend'inin yerine geçmemeli.

Gerekçe:

- ACPX zaten OpenClaw'ın session/turn/event runtime kontratını uyguluyor.
- ACPX, process lease ve temizlik durumunu OpenClaw plugin state sistemiyle entegre ediyor.
- Acp.Net'in en güçlü yanı runtime event streaming değil; process kanıtı, preflight, hata sınıflandırması ve test harness.

Kısa vadeli ürün yolu:

1. Acp.Net tanılama komut kontratını stabilize et.
2. OpenClaw bunu doctor/plugin komutu olarak çağırabilsin.
3. Artifact ve transcript üzerinden environment hatası ile agent hatası ayrımını göster.
4. Derin ACPX entegrasyonuna ancak bu komut kontratı kullanışlı olduktan sonra girilsin.

### 2026-06-09 Spike 013 Tanılama Komutu Notu

`openclaw-acpnet-probe` artık daha stabil bir CLI kontratı sağlıyor.

Desteklenen kararlar:

- stdout tek JSON sonucu için ayrıldı.
- Exit code sözleşmesi netleşti: `0`, `2`, `3`, `64`.
- Sahte-agent varsayılanı korunarak model kotası harcamayan doğrulama yolu sağlandı.
- Gerçek ACP uyumlu komutlar için `--command` ve tekrarlanabilir `--arg` eklendi.
- Araç politikası argümanları `--required-tool` ve `--optional-tool` olarak dışarıdan verilebilir hale geldi.

Bu, OpenClaw core'a dokunmadan önce gerekli ara ürünleşme adımıdır.

### 2026-06-09 Spike 014 Doctor Adapter Notu

Tanılama komutu sonucunun OpenClaw doctor/lint yüzeylerine eşleme kontratı hazırlandı.

Karar:

- `AcpRuntimeDoctorReport` runtime doctor için uygun yüzey.
- `HealthFinding[]` doctor lint için uygun yüzey.
- Opsiyonel araç eksikliği runtime doctor için `ok=true` kalabilir ama lint yüzeyinde `warning` finding olmalıdır.
- Kritik environment hatası doctor ve lint için error olmalıdır.

Bu, Acp.Net'in OpenClaw'a ilk entegrasyonunun doctor/lint kanıtı şeklinde olması gerektiğini güçlendirir.
