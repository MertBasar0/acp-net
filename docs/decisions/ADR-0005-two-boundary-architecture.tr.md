# ADR-0005: İki Sınırlı Mimari — Kuzeyde A2A, Güneyde ACP/Process

> 🇬🇧 English version: [ADR-0005-two-boundary-architecture.md](ADR-0005-two-boundary-architecture.md)

Tarih: 2026-07-10

## Durum

Kabul edildi

## Bağlam

ADR-0004, Training Factory'yi agentic training-ops dogfood alanı olarak yeniden konumlandırdı. Ardından yapılan tasarım tartışması, asıl delegasyon vizyonunu netleştirdi: OpenClaw ana modeli üst seviye bir görev alır ve bunu Isaac Sim'de aksiyon alan bir worker agent'a devreder — worker analiz eder, simülasyonu başlatır, RL koşturur ve hedefe ulaşana kadar döngüye devam eder.

Tarihsel kayıt, deponun bu vizyonun yalnızca **alt yarısını** tanımladığını gösteriyor:

- `openclaw-subagent-runner` örneği delegasyonu "stdio üzerinden yerel bir ACP subagent process'i başlat, tek prompt gönder, tek sonuç topla" olarak kodluyor,
- hiç koşulmamış spike planı `002-sb3-learning-baseline.md` (docs yeniden yapılandırmasında ağaçtan çıkarıldı; ilk commit'ten geri kurtarılabilir) eğitim deneyinin kendisini tanımlıyordu,
- OpenClaw ana modelinin **uzun ömürlü, hedefe kadar döngü kuran, muhtemelen uzak** bir worker'a nasıl ulaşacağını hiçbir doküman tanımlamamış. Depo çapında arama, bu ADR'den önce hiçbir A2A ifadesi bulmuyor.

Yerel process sınırı için ACP seçimi bilinçli ve doğruydu (ADR-0001, ECOSYSTEM: protokol katmanı kalabalık, process katmanı boştu). Kuzey sınır ise hiç tasarlanmamıştı.

İki protokol farklı sınırları çözer:

- **ACP** (Agent Client Protocol): ebeveyn–çocuk, yerel process, stdio; worker'ın ömrü onu başlatan oturuma bağlı. Günlerce sürecek RL döngüsü için yanlış kalıp.
- **A2A** (Agent2Agent): HTTP üzerinden eşler arası, agent card ile keşfedilebilir, görev yaşam döngüsü durumları, uzun süren asenkron işler; worker farklı bir makinede (ör. GPU workstation) yaşayabilir.

Resmî bir A2A .NET SDK'sı mevcut (NuGet'te `A2A` ve `A2A.AspNetCore`, `1.0.0-preview2`); dolayısıyla protokol yüzeyi geliştirilecek bir şey değil, bir bağımlılık.

## Karar

1. **Kuzey sınır = A2A.** OpenClaw ana modeli A2A client olarak davranır; Training-Ops Agent, agent card ve görev yaşam döngüsü yayınlayan bir A2A server'dır. Resmî A2A .NET SDK kullanılır; protokol mekaniği implemente edilmez.
2. **Training-Ops Agent, kendi deposunda yaşayan ayrı ve bilinçli olarak ince bir uygulamadır.** Training Factory'nin gerçek formu budur. Kendine ait kodu şunlarla sınırlıdır: A2A yüzeyi, makine-okunur iş tanımı şeması (YAML/JSON, OSMO tarzı), hedef döngüsü (analiz → konfigüre → başlat → değerlendir → karar ver → tekrarla; iterasyonlar arası kalıcı durumla) ve run-artifact karşılaştırma/eval raporlama.
3. **Güney sınır değişmez.** Worker'ın içinde Isaac Lab / RL eğitimi / araçlar, `Acp.Net.Process` üzerinden başlatılan ve tanılanan yerel process'ler olarak koşar (preflight, WSL path eşleme, transcript, run artifact, hata sınıflandırma); gerektiğinde yerel code agent'lar ACP üzerinden spawn edilir.
4. **Bu depo bir paket ailesi olarak kalır.** Yol haritasındaki Faz 5, spike'ın acp-net tarafındaki izdüşümüdür (dogfood'un hangi kütüphane eksiklerini ortaya çıkardığı); worker'ın evi değildir.

```
OpenClaw ana modeli                    niyet, görev tanımı, süpervizyon
        │  A2A (agent card, görev durumları, uzun ömürlü, uzak-uyumlu)
        ▼
Training-Ops Agent (kendi reposu)      döngünün sahibi: analiz → sim → RL → eval → tekrar
        │  ACP (yerel code agent'lar)  │  düz process'ler (Isaac Lab, RL eğitimi)
        └──── ikisi de Acp.Net process sınırından geçer ────┘
              (preflight, WSL path, transcript, run artifact, hata sınıfları)
```

## Sonuçlar

- A2A'ya geçiş Acp.Net'i değersizleştirmez; kütüphane, yazıldığı sınırda, worker'ın içinde çalışmaya devam eder.
- A2A yatırımı, aynı protokolü hedefleyen Deliberation Lab projesiyle paylaşılır.
- Worker'ın .NET olması Acp.Net dogfood'unu opsiyonel değil yapısal kılar; Python yaprak process'lerde kalır.
- Hiç koşulmamış spike 002 planının yerini, ADR-0004'teki Faz 5 spike tanımı alır — artık A2A yüzeyi de eklenmiş olarak.

## Ne Zaman Yeniden Değerlendirilir

- A2A .NET SDK, 1.0 stable öncesinde server yüzeyini etkileyecek şekilde değişirse,
- Faz 5 spike'ı A2A görev yaşam döngüsünün eğitim döngüsünün durumlarını ifade edemediğini gösterirse,
- OpenClaw birinci sınıf A2A client desteği kazanır ve entegrasyon şekli değişirse,
- worker, bir pakete ait olması gereken sorumluluklar edinirse (o zaman çıkar/ayrıştır; uygulamayı şişirme).
