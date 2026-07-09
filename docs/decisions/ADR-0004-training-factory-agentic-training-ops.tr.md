# ADR-0004: Training Factory Bir Agentic Training-Ops Dogfood Alanıdır

> 🇬🇧 English version: [ADR-0004-training-factory-agentic-training-ops.md](ADR-0004-training-factory-agentic-training-ops.md)

Tarih: 2026-07-09

## Durum

Kabul edildi

## Bağlam

ADR-0000, Training Factory'yi Acp.Net için bir test/dogfooding alanı olarak raflamış ve bir sonraki deneyini Gazebo/PX4 odaklı bir ortamda gerçek SB3 learning baseline'ı olarak tanımlamıştı.

O günden bu yana iki şey değişti.

Acp.Net tarafında:

- `Acp.Net.Process` ve `Acp.Net.Testing` NuGet'te yayında (`0.1.0-alpha.2`),
- Faz 4 gerçek-agent dogfood'u (Gemini CLI ACP modu) tamamlandı.

Sektör tarafında:

- NVIDIA, Physical AI iş yükleri (Isaac Lab, Isaac Sim, GR00T) için production-grade orkestrasyon platformu OSMO'yu açık kaynak yaptı; platform, heterojen donanım üzerinde YAML tabanlı çok aşamalı iş akışları sunuyor.
- OSMO artık coding agent'larla (Claude Code, Codex, Cursor) entegre; agent'lar eğitim operasyonlarını doğrudan yönetebiliyor.
- Isaac Lab; ortam + eğitim yığınını (RSL-RL, SKRL, RL-Games, Stable-Baselines3) multi-GPU ölçekleme, headless çalışma ve yayınlanmış bir referans mimariyle kapsıyor.

Training Factory'nin arkasındaki ürün vizyonu — OpenClaw gibi bir orkestratöre üst seviye bir komut verip robotik projeler için simülasyon, RL eğitimi ve değerlendirmeyi ona yönettirmek — böylece pazar tarafından hem doğrulandı hem de bir incumbent tarafından işgal edildi.

## Karar

1. Training Factory yatay bir ürün değildir. OSMO ile yarışan yeni bir eğitim orkestratörü inşa etme.
2. Training Factory'yi **agentic training-ops dogfood alanı** olarak yeniden tanımla: mevcut yığının üstünde ince, agent'a bakan bir katman — ortam/eğitim olarak Isaac Lab, iş akışı modeli olarak OSMO tarzı makine-okunur iş tanımları, komut veren agent olarak OpenClaw, process sınırında Acp.Net.
3. ADR-0000'daki sıradaki deneyi (Gazebo/PX4 üzerinde gerçek SB3 baseline) emekliye ayır. Yerine Isaac Lab tabanlı spike'ı koy (aşağıda).
4. Farklılaştırıcı odağı Windows workstation + WSL köprüsünde tut: preflight, path eşleme, transcript kaydı, run artifact JSON — bir veri merkezi orkestratörünün umursamadığı platform kiri.

## Spike Tanımı

Hedef efor: yerel GPU erişilebilirliğine bağlı, 2–4 hafta yarı zamanlı çalışma.

- ortam: Isaac Lab, headless, yerelde (WSL veya native Linux)
- eğitim: desteklenen bir RL kütüphanesiyle küçük bir baseline görevi
- orkestrasyon: OpenClaw tek bir üst seviye komut verir; iş, makine-okunur bir tanımla ifade edilir (YAML/JSON, OSMO tarzı)
- process sınırı: .NET'in devrede olduğu her yerde başlatma, preflight ve tanılama Acp.Net üzerinden gider (probe yolu)
- çıktı: run artifact JSON + eval raporu + transcript

Başarı ölçütleri:

- "eğitimi başlat → eval koş → run artifact'i raporla" döngüsü tek komuttan uçtan uca tamamlanır,
- başarısız bir koşu Acp.Net hata sınıflarına (ortam / process / protokol / agent-görev) ayrıştırılabilir,
- aynı komutun tekrarı karşılaştırılabilir bir run artifact üretir (tekrarlanabilirlik > en yüksek skor).

## Sonuçlar

- Yeni bir orkestratör kod tabanı başlatılmaz.
- Training Factory'nin ürünleşme takvimi yoktur; ADR-0000 tahmini (6–12 ay, yüksek entegrasyon riski) ertelenmek yerine emekliye ayrılır.
- Planlanan yerel multi-GPU donanımı (ör. çift RTX 5090) kuyruk/eval/rapor döngüsünün değerini artırır ama bu kararı değiştirmez.
- Çalışma, OSMO'nun ürünleştirdiği agent-güdümlü training-ops yönünün bağımsız bir keşfi olarak portföy değerini korur.

## Ne Zaman Yeniden Değerlendirilir

- yerel multi-GPU donanımı fiilen geldiğinde,
- Isaac Lab spike'ı tamamlanıp tekrarlanabilir bir run artifact ürettiğinde,
- OSMO'nun agent entegrasyonunun Windows workstation + WSL akışlarını kapsadığı (veya kapsamadığı) netleştiğinde,
- ikinci bir gerçek tüketici aynı ince agent katmanını istediğinde.
