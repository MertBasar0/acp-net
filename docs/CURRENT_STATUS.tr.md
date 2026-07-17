# Güncel Durum

> 🇬🇧 English version: [CURRENT_STATUS.md](CURRENT_STATUS.md)

Son güncelleme: 2026-07-17

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
- native/WSL runtime çözümleme (yalnızca bir Windows Store execution-alias stub'ına çözülen düz bir komutu, kilitlenmek yerine WSL'ye yönlendirir)
- Windows/WSL path eşleme
- `AcpProcessSession.ToAgentPath(...)`
- environment değişkeni şekillendirme
- `AdditionalPathEntries`
- zorunlu/opsiyonel executable preflight (Windows Store execution-alias stub'ları bulundu değil, eksik olarak raporlanır)
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

İlk alpha paketleri 2026-06-11 tarihinde, nuget.org trusted publishing (GitHub Actions OIDC) kullanan `publish.yml` workflow'u üzerinden yayınlandı:

- [Acp.Net.Process 0.1.0-alpha.1](https://www.nuget.org/packages/Acp.Net.Process)
- [Acp.Net.Testing 0.1.0-alpha.1](https://www.nuget.org/packages/Acp.Net.Testing)

Sembol paketleri (`.snupkg`) birlikte gönderildi. Paket metadata'sı doğru depo/proje URL'sini ve Apache-2.0 lisans ifadesini taşıyor.

`0.1.0-alpha.2`, 2026-06-13 tarihinde yayınlandı. Taze-tüketici testinde ortaya çıkan bir Windows tuzağını düzeltir: `python3` gibi düz bir komut, bir Microsoft Store execution-alias stub'ına çözülüyor; preflight bunu "bulundu" sayıyor, runner başlatıyor ve sessizce kilitleniyordu. Runtime çözümleyici artık böyle bir komutu WSL'ye yönlendiriyor ve preflight stub'ı "eksik" olarak raporluyor.

Gelecek sürümler aynı elle tetiklenen workflow üzerinden gider; kapı listesi için [RELEASE_CHECKLIST.tr.md](RELEASE_CHECKLIST.tr.md) dosyasına bakın.

## Önemli Bulgular

1. `AgentClientProtocol`, protokol seviyesindeki tipler ve bağlantı davranışı için yararlı olmaya devam ediyor.
2. Acp.Net'in değeri, ACP çevresindeki process/runtime/test/tanılama davranışıdır.
3. WSL path eşleme rastlantısal değildir; gerçek Gemini dogfood'u bunu ortaya çıkardı.
4. Araç preflight'ı önemlidir; eksik `rg`, agent çalışmadan önce tespit edildi.
5. OpenClaw'da runtime backend olarak zaten ACPX var; Acp.Net onun yerine geçmemeli.
6. Resmî bir öneri hazırlanmadıkça OpenClaw entegrasyonu referans/tanılama düzeyinde kalmalı.

## Training Factory Yönü

Training Factory (ilişkili RL eğitim projesi) 2026-07-09 tarihinde yeniden konumlandırıldı: Isaac Lab ve OSMO tarzı iş tanımlarının üstünde, komut veren agent olarak OpenClaw ve process sınırında Acp.Net ile çalışan bir agentic training-ops dogfood alanıdır. Açıkça yatay bir orkestratör ürünü değildir. Karar, spike tanımı ve başarı ölçütleri [ADR-0004](decisions/ADR-0004-training-factory-agentic-training-ops.tr.md) dosyasında; spike'ın kendisi [yol haritasında](ROADMAP.tr.md) Faz 5 olarak yer alıyor ve yerel GPU erişilebilirliğine bağlı.

[ADR-0005](decisions/ADR-0005-two-boundary-architecture.tr.md) (2026-07-10) mimariyi sabitledi: OpenClaw ana modeli worker'a A2A üzerinden ulaşır (kuzey sınır, resmî A2A .NET SDK); worker — Training Factory'nin gerçek formu olan Training-Ops Agent — kendi deposunda yaşayan ayrı ve ince bir uygulamadır; içinde process'ler ve yerel code agent'lar Acp.Net/ACP sınırından geçer (güney). Bu depo bir paket ailesi olarak kalır; ne worker'ı ne de protokol mekaniğini barındırır.

## Güncel Riskler

- API hâlâ alpha seviyesinde.
- Tanılama CLI'ı bilinçli olarak hâlâ sample/araç; paketlenmiş bir ürün değil.
- Bazı Windows + WSL kurulumlarında, Windows interop executable'larını çağıran bir Node child process WSL interop sınırında başarısız olabilir (`UtilBindVsockAnyPort`); herhangi bir OpenClaw entegrasyon çalışmasından önce bu yol ayrıca doğrulanmalı.

## Mühendislik Notları Arşivi

Tarihli spike raporları (001–014) ve günlük handoff notları 2026-06-10 tarihinde git geçmişinden çıkarıldı. Depo kökündeki, git tarafından takip edilmeyen `notes/` klasöründe (`notes/handoffs/`, `notes/spikes/`) yaşıyorlar; bu klasör GitHub uzak deposuna gönderilmez. Bu çalışmalardan damıtılan kalıcı kararlar [decisions/](decisions/) altında yaşıyor.
