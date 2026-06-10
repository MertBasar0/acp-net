# Geliştirme Rehberi

> 🇬🇧 English version: [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md)

Son güncelleme: 2026-06-10

## Gereksinimler

- .NET 8 SDK
- Node.js (yalnızca OpenClaw doctor adapter taslak doğrulayıcısı için)
- agent runtime'ında erişilebilir `python3` (sahte ACP agent ve varsayılan probe bunu kullanır)

Geliştirme Windows + WSL üzerinde doğrulanmıştır; aynı komutlar düz bir Linux kurulumunda da çalışır.

## Windows + WSL Path Notu

Bazı Windows + WSL kurulumlarında, WSL içinde erişilebilir olan `dotnet` komutu aslında Windows `dotnet.exe`'dir. Windows MSBuild, `/home/<kullanıcı>/...` gibi Linux tarzı mutlak yolları yorumlayamaz ve bunları geçersiz anahtar (switch) olarak değerlendirebilir.

Böyle bir durumda WSL dosyalarını `dotnet`'e UNC path olarak verin:

```bash
dotnet test '\\wsl.localhost\<Dağıtım>\<depo-yolu>\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

`<Dağıtım>` yerine WSL dağıtımınızın adını (örneğin `Ubuntu`), `<depo-yolu>` yerine deponun WSL içindeki konumunu yazın. Alternatif olarak WSL içine native Linux .NET SDK kurup düz göreli yolları kullanabilirsiniz.

Aşağıdaki tüm komutlar, depo kökünden ve depo yolunu anlayan bir `dotnet` ile çalıştırıldıkları varsayımıyla yazılmıştır.

## Sık Kullanılan Komutlar

Testleri çalıştır:

```bash
dotnet test src/acp-net/AcpNetMvp.slnx --logger "console;verbosity=minimal"
```

Paketle:

```bash
dotnet pack src/acp-net/Acp.Net.Process/Acp.Net.Process.csproj --output artifacts/packages
dotnet pack src/acp-net/Acp.Net.Testing/Acp.Net.Testing.csproj --output artifacts/packages
```

Tanılama probe'unu sahte agent ile çalıştır:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj
```

Doctor adapter taslak doğrulayıcısını çalıştır:

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

## Üretilen Dosyalar

Git tarafından yok sayılanlar:

- `bin/`
- `obj/`
- `artifacts/`
- `*.ndjson`

Belirli bir fixture bilinçli olarak `docs/contracts` altına konmadıkça; yerel transcript'leri, paket çıktılarını veya üretilmiş run artifact'lerini commit'lemeyin.

## Dokümantasyon Kuralları

- Dokümantasyonun ana dili İngilizcedir.
- Her çekirdek dokümanın yanında `.tr.md` son ekiyle tam bir Türkçe sürümü bulunur; düzenlerken ikisini senkron tutun.
- Dokümantasyona makineye özel mutlak yollar koymayın; depo-göreli yollar ve `<Dağıtım>` gibi yer tutucular kullanın.
- Tarihli çalışma notları (spike oturum raporları, günlük handoff'lar) git geçmişine ait değildir; depo kökündeki git tarafından takip edilmeyen `notes/` klasöründe tutulurlar. Kalıcı sonuçları `docs/decisions/` altında ADR olarak kaydedin.
- `docs/CURRENT_STATUS.md`, proje durumu için tek doğruluk kaynağıdır; durum veya test sayısı kopyalamak yerine ona bağlantı verin.

## Kodlama İlkeleri

- Kesinlikle gerekli olmadıkça protokol şema sahipliğini Acp.Net dışında tut.
- Protokol seviyesindeki davranış için `AgentClientProtocol` ile birlikte çalışmayı tercih et.
- Üretim runtime'ı ile test yardımcılarını ayrı tut.
- Kullanımı kanıtlanana kadar tanılama CLI/araçlarını opsiyonel tut.
- Özellikleri başarısız olan testlerden veya somut dogfood kanıtlarından türet.
