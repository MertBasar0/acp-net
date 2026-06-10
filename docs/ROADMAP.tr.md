# Yol Haritası

> 🇬🇧 English version: [ROADMAP.md](ROADMAP.md)

Son güncelleme: 2026-06-10

## Ürün Hedefi

Acp.Net'i bağımsız bir .NET paket ailesi olarak yayınlamak:

- `Acp.Net.Process`
- `Acp.Net.Testing`

Tanılama (diagnostics) şimdilik depo aracı olarak kalıyor. Gelecekte bir `Acp.Net.Diagnostics` paketi mümkün, ancak ilk alpha paket turunun parçası değil.

OpenClaw, varsayılan implementasyon hedefi değil, referans entegrasyon olarak kalmalı.

## Faz 0: Depo Bağımsızlığı

Durum: tamamlandı.

Hedefler:

- projeyi OpenClaw workspace'inden çıkarmak
- git'i başlatmak
- bağımsız GitHub deposuna push'lamak
- güncel durumu ve sonraki adımları dokümante etmek
- spike geçmişini korumak (artık depo kökündeki git tarafından takip edilmeyen `notes/` klasöründe tutuluyor; kalıcı sonuçlar `docs/decisions/` altında)

## Faz 1: Alpha Paket Sağlamlaştırma

Hedefler:

- public API isimlendirmesini gözden geçir
- paket sürümleme politikasına karar ver
- paket README kapsamını ekle
- XML doc veya minimal API dokümantasyonu ekle
- `dotnet test` ve `dotnet pack`'in tek komutla tekrarlanabilir olmasını sağla
- CI workflow ekle
- lisans seçildi: Apache-2.0

Aday çıktı:

- `Acp.Net.Process.0.1.0-alpha.1`
- `Acp.Net.Testing.0.1.0-alpha.1`

## Faz 2: Tanılama Şekli

Hedefler:

- tanılamayı şimdilik sample/araç olarak tut
- `openclaw-acpnet-probe` CLI kontratını stabilize et
- stdout'u tek JSON sonucu olarak koru
- stderr'i yalnızca tanılama için kullan
- exit code'ları kesinleştir
- doctor/lint eşlemesini dokümante et

Bu fazda OpenClaw core koduna dokunma.

Bu fazda `Acp.Net.Diagnostics` paketi oluşturma.

## Faz 3: Referans Entegrasyonlar

Hedefler:

- `docs/integrations/openclaw.md` yaz
- OpenClaw entegrasyonunu üçüncü taraf/referans sample olarak tut
- bir OpenClaw health check'inin tanılama komutunu nasıl çağırabileceğini dokümante et
- runtime backend değişiminden kaçın
- ancak paket stabil olduktan sonra bakımcı-dostu öneri hazırla

## Faz 4: Gerçek Agent Dogfood

Hedefler:

- Claude kotası kullanmadan kontrollü Gemini ACP dogfood'u çalıştır
- transcript'leri opsiyonel yerel kanıt olarak sakla
- özel `--command` tanılama yolunu doğrula
- eksik/mevcut araç senaryolarını test et

## Faz 5: Training Factory'ye Dönüş

Training Factory rafta kalmaya devam ediyor.

Ancak Acp.Net alpha paketleme netleştikten sonra yeniden ele alınmalı.

Sıradaki yararlı Training Factory spike'ı:

- random rollout değil
- gerçek bir SB3 öğrenme baseline'ı kullan
- öğrenmenin baseline'ı geçip geçmediğini kanıtla

## Şimdilik Açık Hedef-Dışılar

- tam ACP protokol SDK'sının yerine geçmek
- OpenClaw core PR'ı
- ACPX'in yerine geçmek
- Training Factory ürünleştirmesi
- UI/dashboard
- sağlayıcı pazaryeri
