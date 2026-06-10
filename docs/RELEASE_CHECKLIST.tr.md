# Sürüm Kontrol Listesi

> 🇬🇧 English version: [RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md)

Son güncelleme: 2026-06-10

Herhangi bir alpha NuGet yayınından önce bunu kullanın. Güncel proje durumu için bkz. [CURRENT_STATUS.tr.md](CURRENT_STATUS.tr.md).

## Güncel Yayın Kararı

Proje sahibinden açık bir yayın kararı olmadan NuGet'e yayın yapma.

Depo yerel alpha paketleri üretebiliyor ve Apache-2.0 seçildi. Herkese açık NuGet yayını için hâlâ son bir paket ID uygunluk onayı, bir NuGet API anahtarı ve son sürüm commit'i üzerinde paket metadata yeniden kontrolü gerekiyor.

## Alpha Öncesi Gerekenler

- Public API isimleri gözden geçirildi.
- Paket açıklamaları gözden geçirildi.
- Lisans proje sahibi tarafından seçildi.
- Yayın workflow'u için NuGet API anahtarı oluşturuldu.
- GitHub depo secret'ı `NUGET_API_KEY` yapılandırıldı.
- Yayından hemen önce paket ID uygunluğu kontrol edildi.
- README örnekleri taze bir klondan test edildi.
- `dotnet test` geçiyor.
- `Acp.Net.Process` için `dotnet pack` geçiyor.
- `Acp.Net.Testing` için `dotnet pack` geçiyor.
- Üretilen paketler yerelde incelendi.
- Sembol paketleri yerelde incelendi.
- CI geçiyor.
- Depo görünürlüğü sürüm fazı için bilinçli olarak ayarlandı.

## Doğrulama Komutları

Depo kökünden:

```bash
dotnet test src/acp-net/AcpNetMvp.slnx --logger "console;verbosity=minimal"
dotnet pack src/acp-net/Acp.Net.Process/Acp.Net.Process.csproj --output artifacts/packages
dotnet pack src/acp-net/Acp.Net.Testing/Acp.Net.Testing.csproj --output artifacts/packages
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

Windows + WSL kurulumları için [DEVELOPMENT_GUIDE.tr.md](DEVELOPMENT_GUIDE.tr.md) içindeki path notuna bakın.

## Şu Durumlarda Yayınlama

- Herhangi bir public API hâlâ sample görünümündeyse.
- OpenClaw tanılama davranışı core entegrasyonu olarak tanımlanıyorsa.
- Paketin `AgentClientProtocol` karşısındaki kimliği hâlâ belirsizse.
- Paket, üretilmiş artifact'ler veya yerel transcript'ler içeriyorsa.
- Komut kontratı stabil olmadan tanılama paketleniyorsa.

## Güncel Metadata Durumu

- `RepositoryUrl`: `https://github.com/MertBasar0/acp-net`
- `PackageProjectUrl`: `https://github.com/MertBasar0/acp-net`
- `RepositoryType`: `git`
- `PackageLicenseExpression`: `Apache-2.0`
- `SymbolPackageFormat`: `snupkg`
- Yayınlanmış paketler: yok

## Paket ID Kontrolü

2026-06-10 tarihinde NuGet flat-container API üzerinden kontrol edildi:

- `https://api.nuget.org/v3-flatcontainer/acp.net.process/index.json`: 404
- `https://api.nuget.org/v3-flatcontainer/acp.net.testing/index.json`: 404

Yorum: kontrol anında bu paket ID'leri yayınlanmamıştı. `dotnet nuget push`'tan hemen önce yeniden kontrol edin.

## GitHub Actions İle Yayın

Yayın workflow dosyası:

```text
.github/workflows/publish.yml
```

Workflow elle tetiklenir ve `confirm_publish=publish` gerektirir. `NUGET_API_KEY` depo secret'ını kullanır.

## Önerilen Yayın Sırası

1. Test ve pack'i yeniden çalıştır.
2. Paket ID'lerini yeniden kontrol et.
3. `.nupkg` paketlerini push'la.
4. `.snupkg` sembol paketlerini push'la.
5. NuGet paket sayfalarının Apache-2.0, README, depo URL'si ve prerelease sürümünü gösterdiğini doğrula.
6. GitHub depo görünürlüğünün sürüm kararıyla uyumlu olduğunu doğrula.
