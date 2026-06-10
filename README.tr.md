# Acp.Net

> 🇬🇧 English version of this document: [README.md](README.md)

Acp.Net, ACP uyumlu agent process'lerini çalıştırmak, test etmek ve tanılamak için geliştirilmiş bir .NET ürün ailesidir.

Tam bir ACP protokol SDK'sı **değildir** ve bir OpenClaw core fork'u değildir. Güncel ürün yönü:

> Acp.Net bağımsız bir paket ailesi olarak kalmalı. OpenClaw, ürün sınırı olarak değil; referans tüketici ve dogfood ortamı olarak kullanılmalı.

## Ne Sağlar

Şu anda gerçeklenmiş yüzeyler:

- `Acp.Net.Process`: process runner, WSL/native runtime köprüsü, path eşleme, environment şekillendirme, preflight kontrolleri, transcript kaydı, run artifact üretimi, kapatma (shutdown) politikası.
- `Acp.Net.Testing`: entegrasyon testleri için deterministik sahte (fake) ACP agent script'leri ve transcript doğrulamaları.
- Tanılama sample/araçları: OpenClaw odaklı probe komutu ve doctor/lint eşleme taslağı. Bunlar bilinçli olarak henüz ayrı bir NuGet paketi değildir.

Çekirdek değer, şu hata sınıflarını birbirinden ayırmaktır:

- environment hatası: eksik araçlar, yanlış PATH, yanlış runtime, WSL/path sorunu
- process hatası: başlatma, çıkış, timeout, kapatma
- protokol hatası: ACP/JSON-RPC akış sorunu
- agent hatası: agent çalıştı ama devredilen görevde başarısız oldu

## AgentClientProtocol İle İlişkisi

Acp.Net, mevcut `AgentClientProtocol` paketini tamamlar.

`AgentClientProtocol`, protokol tipleri ve JSON-RPC client/agent bağlantı davranışı için kullanışlıdır. Acp.Net ise bu protokolün çevresindeki pratik runtime katmanına odaklanır:

- agent process'ini başlatmak
- environment değişkenlerini ve PATH'i şekillendirmek
- Windows/WSL path'lerini eşlemek
- başlatmadan önce gerekli araçları kontrol etmek
- ham stdio ve yaşam döngüsü olaylarını kaydetmek
- makine tarafından okunabilir run artifact'leri üretmek
- process sınırı davranışını sahte agent'larla test etmek

## Depo Yapısı

- `src/acp-net/Acp.Net.Process/`: üretim runtime paketi.
- `src/acp-net/Acp.Net.Testing/`: test yardımcıları.
- `src/acp-net/Acp.Net.UnitTests/`: birim testleri.
- `src/acp-net/Acp.Net.IntegrationTests/`: process sınırı entegrasyon testleri.
- `src/samples/`: örnek tüketiciler ve probe'lar.
- `src/openclaw-probe/`: OpenClaw odaklı tanılama/doctor adapter taslakları.
- `docs/decisions/`: ADR'ler ve ürün kararları.
- `docs/product/`: ürün tasarım notları.
- `docs/contracts/`: JSON/sonuç kontratları ve adapter eşleme fixture'ları.

Tarihli spike raporları ve günlük handoff notları, bu deponun dışında, bakımcının yerel mühendislik notları olarak tutulmaktadır. Bu çalışmaların kalıcı sonuçları `docs/decisions/` altında kayıtlıdır.

## Hızlı Doğrulama

Depo kökünden:

```bash
dotnet test src/acp-net/AcpNetMvp.slnx --logger "console;verbosity=minimal"
```

Tüm birim ve entegrasyon testleri geçmelidir.

Model kotası harcamadan tanılama probe'unu çalıştırın (deterministik sahte ACP agent kullanır; agent runtime'ında `python3` gerektirir):

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj
```

Probe, stdout'a tek bir JSON sonucu basar ve başarı durumunda `0` koduyla çıkar.

OpenClaw doctor adapter taslağını doğrulayın (Node.js gerektirir):

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

Beklenen:

```text
doctor adapter scenarios ok (4)
```

> **Windows + WSL notu:** Windows `dotnet.exe`'yi WSL dosya sistemi içindeki proje dosyalarıyla çalıştırıyorsanız, dosyaları UNC path olarak verin (`\\wsl.localhost\<Dağıtım>\...`). Ayrıntılar için [docs/DEVELOPMENT_GUIDE.md](docs/DEVELOPMENT_GUIDE.md) dosyasına bakın.

## Dokümantasyon

Çalışmaya dönerken önce şunları okuyun:

- [Güncel Durum](docs/CURRENT_STATUS.tr.md) — proje durumu için tek doğruluk kaynağı
- [Yol Haritası](docs/ROADMAP.tr.md)
- [Geliştirme Rehberi](docs/DEVELOPMENT_GUIDE.tr.md)
- [OpenClaw Entegrasyon Stratejisi](docs/OPENCLAW_INTEGRATION_STRATEGY.tr.md)
- [Sürüm Kontrol Listesi](docs/RELEASE_CHECKLIST.tr.md)
- [Ürün Tasarımı](docs/product/acp-net-mvp-product-design.tr.md)
- [Kararlar (ADR'ler)](docs/decisions/)

Tüm çekirdek dokümanların İngilizce ana sürümleri aynı klasörde, `.tr.md` son eki olmadan bulunur.

## Yakın Vadeli Yön

Önerilen sıradaki iş bir OpenClaw core PR'ı değildir. Sıradaki iş, Acp.Net'i bağımsız bir paket ailesi olarak sağlamlaştırmaktır:

1. paket API sınırlarını stabilize et,
2. komut kontratı daha fazla kullanım kanıtı toplayana kadar tanılamayı depo aracı olarak tut,
3. doküman ve örnekleri iyileştir,
4. ilk alpha NuGet paketlerini hazırla,
5. OpenClaw entegrasyonunu referans/dogfood malzemesi olarak koru.

Training Factory, henüz kanıtlanmamış ve sonraya bırakılmış bir ürün hattıdır. Mevcut Acp.Net paketleme kararlarını yönlendirmemelidir.

## Lisans

Acp.Net, Apache License 2.0 ile lisanslanmıştır. Bkz. [LICENSE](LICENSE).
