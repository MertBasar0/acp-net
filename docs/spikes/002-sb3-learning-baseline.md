# Spike 002: SB3 Learning Baseline

## Amac

Training Factory icin "ortam calisiyor" degil, "ajan bir sey ogreniyor" sorusunu test etmek.

## Kapsam

Minimum uygulanabilir test:

- Stable-Baselines3 ile bilinen bir drone/RL ortaminda egitim.
- Random baseline ile egitilmis policy karsilastirmasi.
- Sabit seed ile tekrarlanabilir sonuc.
- Eval episode'lari train episode'larindan ayrilir.
- Reward egrisi ve final eval metrikleri kaydedilir.

Tercih edilen ilk aday: `gym-pybullet-drones` veya benzer, kurulumu nispeten hafif bir drone RL ortami.

## Kapsam disi

- PX4/Gazebo tam entegrasyonu.
- Gercek zamanli simulator kararliligi.
- Multi-agent swarm senaryolari.
- UI/dashboard.
- Urunlesme.

Bu maddeler ancak baseline learning sinyali gorulurse sonraki faza alinir.

## Kabul kriterleri

Spike tamamlanmis sayilmasi icin:

1. Random baseline skoru olculur.
2. SB3 policy ayni eval setinde random baseline'i anlamli sekilde gecer veya gecemedigi net raporlanir.
3. Komutlar, seed, paket surumleri ve metrikler kaydedilir.
4. Sonuc yeniden kosulabilir halde dokumante edilir.

Basarisiz sayilacak durumlar:

- Sadece random rollout calismasi.
- Sadece ortam kurulumu.
- Sadece tek episode gorsel demo.
- Eval olmadan train reward yorumlamak.

## Karar etkisi

Baseline gecilirse Training Factory, Acp.Net sonrasi ikinci hat olarak tutulabilir.

Baseline gecilemezse Training Factory urun hipotezi rafa kaldirilir; sadece Acp.Net'in ilerideki demo/test senaryolari icin kullanilir.

## Zaman kutusu

2-4 hafta. Bu aralik Training Factory'nin entegrasyon riskini gostermek icin yeterlidir; daha uzun sure urunlesme yatirimi sayilir.
