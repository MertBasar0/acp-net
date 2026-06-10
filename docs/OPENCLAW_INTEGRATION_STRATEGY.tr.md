# OpenClaw Entegrasyon Stratejisi

> 🇬🇧 English version: [OPENCLAW_INTEGRATION_STRATEGY.md](OPENCLAW_INTEGRATION_STRATEGY.md)

Son güncelleme: 2026-06-10

## Karar

OpenClaw core entegrasyonunu ana ürün yolu olarak ele alma.

Acp.Net bağımsız bir paket ailesi olarak geliştirilmeli. OpenClaw, referans tüketici ve dogfood hedefi olarak kalmalı.

## Neden

OpenClaw'da ACP runtime backend davranışı için zaten `extensions/acpx` var:

- session oluşturma
- turn streaming
- runtime olayları
- process lease durumu
- temizlik/reaper davranışı
- doctor kancaları

Acp.Net, ACPX'in yerine geçmemeli.

Acp.Net'in ayırt edici değeri:

- process runtime kanıtı
- preflight kontrolleri
- WSL/path eşleme
- transcript kaydı
- run artifact JSON
- hata sınıflandırması
- deterministik process sınırı testleri

## Kabul Edilebilir Entegrasyon Şekilleri

### Tercih Edilen: Üçüncü Taraf Referans Entegrasyonu

OpenClaw'ın bir Acp.Net tanılama komutunu nasıl çağırabileceğini gösteren dokümantasyon ve sample'lar sağla.

Bu, kullanıcı seçimini korur ve .NET'i OpenClaw core'a zorlamaktan kaçınır.

### Sonrası İçin Kabul Edilebilir: Doctor/Lint Adapter Önerisi

Yalnızca paket stabilizasyonundan sonra, küçük bir adapter yolu öner:

- harici tanılama komutunu çağır
- tek JSON sonucunu parse et
- `HealthFinding[]`'e eşle
- ACPX'in yerine geçme

### Önerilmeyen: ACPX'in Yerine Geçmek

Bu, mevcut OpenClaw runtime sorumluluklarını tekrarlar ve zor bir bakımcı yükü yaratır.

## Bakımcı-Dostu Kural

Bir gün bir OpenClaw PR'ı önerilirse, küçük ve entegrasyon-noktası odaklı olmalı:

- geniş kapsamlı runtime yeniden yazımı yok
- core'da zorunlu .NET bağımlılığı yok
- ACPX değişimi yok
- gizli process davranış değişiklikleri yok
- tüm davranışlar opt-in konfigürasyon veya plugin/sample yolunun arkasında

## Güncel Referans Artifact'leri

Bu depoda:

- `src/samples/openclaw-acpnet-probe/` — stabilize edilmiş tanılama komut probe'u
- `src/openclaw-probe/doctor-adapter-draft.mjs` — doctor/lint adapter taslağı
- `src/openclaw-probe/verify-doctor-adapter-draft.mjs` — adapter senaryo doğrulayıcısı
- `docs/contracts/openclaw-doctor-adapter-draft.md` — eşleme kontratı
- `docs/contracts/openclaw-doctor-mapping.scenarios.json` — senaryo fixture'ları

Bu artifact'leri üreten tarihli spike raporları (011–014), deponun dışında bakımcının yerel mühendislik notları olarak tutulmaktadır. Kalıcı sonuçları [decisions/ADR-0002](decisions/ADR-0002-independent-package-openclaw-reference.md) ve [decisions/ADR-0003](decisions/ADR-0003-diagnostics-remains-tooling.md) altında kayıtlıdır.
