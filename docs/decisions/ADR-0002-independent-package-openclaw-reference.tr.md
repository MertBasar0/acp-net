# ADR-0002: Bağımsız Paket, Referans Tüketici Olarak OpenClaw

> 🇬🇧 English version: [ADR-0002-independent-package-openclaw-reference.md](ADR-0002-independent-package-openclaw-reference.md)

Tarih: 2026-06-10

## Durum

Kabul edildi.

## Bağlam

Spike 011–014 sürecinde Acp.Net, OpenClaw entegrasyon ihtiyaçlarına karşı değerlendirildi. Spike oturum raporları, depo kökündeki, git tarafından takip edilmeyen `notes/` klasöründe tutulmaktadır; bu klasör uzak depoya gönderilmez.

OpenClaw'da `extensions/acpx` altında zaten ACPX runtime desteği var. Bu runtime; session/turn semantiğine, process lease durumuna ve temizlik davranışına sahiptir. Acp.Net'i doğrudan OpenClaw core'a eklemek, ACPX sorumluluklarını tekrarlama riski taşır ve zor bir bakımcı incelemesi gerektirirdi.

Aynı zamanda Acp.Net, OpenClaw'a özgü olmayan bir değer ortaya koydu:

- process başlatma ve kapatma
- native/WSL path eşleme
- environment ve PATH şekillendirme
- zorunlu/opsiyonel executable preflight
- transcript kaydı
- run artifact üretimi
- process sınırında sahte agent testleri
- tanılama komutu çıktısı

## Karar

Acp.Net, bağımsız bir .NET paket ailesi olarak geliştirilecek:

- `Acp.Net.Process`
- `Acp.Net.Testing`

Tanılama şimdilik depo aracı olarak kalıyor. Olası bir gelecekteki `Acp.Net.Diagnostics` paketi ayrıca ADR-0003'te ele alınıyor.

OpenClaw; ana ürün sınırı olarak değil, referans tüketici ve dogfood ortamı olarak ele alınacak.

## Sonuçlar

Olumlu:

- Acp.Net, OpenClaw dışında da kullanışlı olabilir.
- Kullanıcılar core runtime değişikliklerini kabul etmek yerine pakete kendi istekleriyle geçebilir.
- OpenClaw için bakımcı yükü azalır.
- Ürün mesajı netleşir.
- NuGet paketleme ve dokümanlar ana teslimat yolu olur.

Olumsuz:

- OpenClaw entegrasyonu sonraki bir adım olarak kalır.
- Entegrasyon noktaları resmîleşene kadar sample/adapter kodunda bir miktar tekrar olabilir.
- Tanılama komutunun paketlenmesi bilinçli olarak ertelenmiştir.

## Takip

Herhangi bir OpenClaw entegrasyon PR'ından önce, sıradaki iş Acp.Net'i paket ailesi olarak sağlamlaştırmak olmalı:

1. README ve paket dokümanlarını tamamla,
2. public API'yi temizle,
3. CI ekle,
4. lisansa karar ver,
5. alpha paketleri yayınla,
6. OpenClaw entegrasyonunu sample/öneri olarak koru.
