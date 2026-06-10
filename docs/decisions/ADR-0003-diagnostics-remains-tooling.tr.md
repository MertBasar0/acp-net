# ADR-0003: Tanılama Şimdilik Araç Olarak Kalsın

> 🇬🇧 English version: [ADR-0003-diagnostics-remains-tooling.md](ADR-0003-diagnostics-remains-tooling.md)

Tarih: 2026-06-10

## Durum

Kabul edildi.

## Bağlam

Acp.Net artık OpenClaw odaklı bir tanılama probe'una ve doctor/lint eşleme taslaklarına sahip. Bunlar yararlı kanıtlar, ancak ayrı bir NuGet paketi olarak yayınlanacak kadar stabil değiller.

Güncel stabil paket adayları:

- `Acp.Net.Process`
- `Acp.Net.Testing`

## Karar

İlk alpha paket turu için `Acp.Net.Diagnostics` oluşturma.

Tanılamayı depo aracı ve sample olarak tut:

- `src/samples/openclaw-acpnet-probe/`
- `src/openclaw-probe/`
- `docs/contracts/openclaw-doctor-adapter-draft.md`
- `docs/contracts/openclaw-doctor-mapping.scenarios.json`

Tanılama komutu kontratı bilinçli olarak küçük kalmalı:

- stdout, makine tarafından okunabilir tek bir JSON sonucu basar
- stderr yalnızca tanılama/yardım metni içindir
- exit code'lar araç çağıranlar için yeterince stabil kalır
- run artifact'leri ve transcript'ler yerel kanıt olarak kalır
- OpenClaw eşlemesi core entegrasyonu değil, adapter taslağı olarak kalır

## Sonuçlar

- İlk alpha paket kapsamı daha dar kalır.
- Tanılama, NuGet uyumluluk baskısı olmadan evrilebilir.
- OpenClaw entegrasyonu opt-in ve harici kalır.
- Tekrarlanan kullanım kontratı kanıtladıktan sonra gelecekte bir `Acp.Net.Diagnostics` paketi hâlâ mümkündür.

## Yeniden Değerlendirme Koşulları

- en az iki sample-dışı tüketici aynı tanılama komut kontratına ihtiyaç duyduğunda,
- OpenClaw entegrasyon şekli netleştiğinde,
- exit code'lar ve JSON sonuç şeması değişmeyi bıraktığında,
- tanılamanın `Acp.Net.Process` ve `Acp.Net.Testing` ile karşılaştırılabilir testleri olduğunda.
