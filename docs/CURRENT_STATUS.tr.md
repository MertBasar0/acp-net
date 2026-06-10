# Güncel Durum

> 🇬🇧 English version: [CURRENT_STATUS.md](CURRENT_STATUS.md)

Son güncelleme: 2026-06-10

Bu doküman, proje durumu için tek doğruluk kaynağıdır. Diğer dokümanlar durum bilgisini tekrarlamak yerine buraya bağlantı verir.

## Ürün Konumu

Acp.Net; ACP uyumlu agent process runtime'ı, tanılama ve test için bağımsız bir .NET paket ailesidir.

Güncel karar:

> Acp.Net'i bağımsız bir paket ailesi olarak inşa et. OpenClaw core entegrasyonunu ürün hedefi yapma.

OpenClaw şu açılardan yararlıdır:

- gerçek bir referans tüketici,
- bir dogfood ortamı,
- somut runtime gereksinimlerinin kaynağı,
- gelecekte bir örnek entegrasyon.

OpenClaw core değişiklikleri varsayılan geliştirme yolu değil, sonraya bırakılmış bir öneri olarak ele alınmalıdır.

## Gerçeklenmiş Kod

Solution:

`src/acp-net/AcpNetMvp.slnx`

Projeler:

- `Acp.Net.Process`
- `Acp.Net.Testing`
- `Acp.Net.UnitTests`
- `Acp.Net.IntegrationTests`

Sample/araçlar:

- `src/samples/acp-process-with-agentclientprotocol/`
- `src/samples/acp-process-with-gemini/`
- `src/samples/openclaw-subagent-runner/`
- `src/samples/openclaw-acpnet-probe/`
- `src/openclaw-probe/`

## Gerçeklenmiş Kabiliyetler

- `AcpProcessRunner` ile process başlatma
- native/WSL runtime çözümleme
- Windows/WSL path eşleme
- `AcpProcessSession.ToAgentPath(...)`
- environment değişkeni şekillendirme
- `AdditionalPathEntries`
- zorunlu/opsiyonel executable preflight
- uyarı (warning) ile hızlı-başarısızlık (fail-fast) preflight politikası ayrımı
- `AcpPreflightException`
- run hatası sınıflandırması
- transcript kaydı
- run artifact JSON
- önce nazik kapatma (graceful shutdown), sonra zorla sonlandırma (hard kill)
- başlatma iptal edildiğinde başlatılmış process'in temizlenmesi
- deterministik sahte ACP agent
- asılı kalan (hanging) sahte agent fixture'ı
- transcript doğrulamaları
- otomatik exit-code kontrat testleriyle OpenClaw tarzı tanılama probe'u
- OpenClaw doctor/lint eşleme taslağı

## Doğrulama

Depo kökünden:

```bash
dotnet test src/acp-net/AcpNetMvp.slnx --logger "console;verbosity=minimal"
dotnet pack src/acp-net/Acp.Net.Process/Acp.Net.Process.csproj --output artifacts/packages
dotnet pack src/acp-net/Acp.Net.Testing/Acp.Net.Testing.csproj --output artifacts/packages
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

Tüm testler geçmeli, her iki paket başarıyla paketlenmeli ve adapter doğrulayıcısı `doctor adapter scenarios ok (4)` çıktısını vermelidir.

Windows + WSL kurulumlarında, WSL path'lerinin Windows `dotnet.exe`'ye nasıl verileceği için [DEVELOPMENT_GUIDE.tr.md](DEVELOPMENT_GUIDE.tr.md) dosyasına bakın.

## NuGet Durumu

Yerel paket üretimi şunlar için çalışıyor:

- `Acp.Net.Process.0.1.0-alpha.1`
- `Acp.Net.Testing.0.1.0-alpha.1`

Hiçbir paket yayınlanmadı. Paket metadata'sı doğru depo/proje URL'sini ve Apache-2.0 lisans ifadesini taşıyor. Yayın için hâlâ gerekenler:

- proje sahibinden açık bir yayın kararı,
- bir NuGet API anahtarı ve `NUGET_API_KEY` depo secret'ı,
- push'tan hemen önce son bir paket ID uygunluk kontrolü.

Tam kapı listesi için [RELEASE_CHECKLIST.tr.md](RELEASE_CHECKLIST.tr.md) dosyasına bakın.

## Önemli Bulgular

1. `AgentClientProtocol`, protokol seviyesindeki tipler ve bağlantı davranışı için yararlı olmaya devam ediyor.
2. Acp.Net'in değeri, ACP çevresindeki process/runtime/test/tanılama davranışıdır.
3. WSL path eşleme rastlantısal değildir; gerçek Gemini dogfood'u bunu ortaya çıkardı.
4. Araç preflight'ı önemlidir; eksik `rg`, agent çalışmadan önce tespit edildi.
5. OpenClaw'da runtime backend olarak zaten ACPX var; Acp.Net onun yerine geçmemeli.
6. Resmî bir öneri hazırlanmadıkça OpenClaw entegrasyonu referans/tanılama düzeyinde kalmalı.

## Güncel Riskler

- API hâlâ alpha seviyesinde.
- Tanılama CLI'ı bilinçli olarak hâlâ sample/araç; paketlenmiş bir ürün değil.
- WSL içinden Windows interop executable'larını çağıran bir Node child process, sandbox'lı bir ortamda başarısız oldu (`UtilBindVsockAnyPort`); herhangi bir OpenClaw entegrasyon çalışmasından önce sandbox dışında yeniden doğrulanmalı.
- Henüz hiçbir NuGet yayını yapılmadı.
- Training Factory kanıtlanmamış durumda ve MVP yolunun dışında kalmalı.

## Mühendislik Notları Arşivi

Tarihli spike raporları (001–014) ve günlük handoff notları 2026-06-10 tarihinde git geçmişinden çıkarıldı. Depo kökündeki, git tarafından takip edilmeyen `notes/` klasöründe (`notes/handoffs/`, `notes/spikes/`) yaşıyorlar; bu klasör GitHub uzak deposuna gönderilmez. Bu çalışmalardan damıtılan kalıcı kararlar [decisions/](decisions/) altında yaşıyor.
