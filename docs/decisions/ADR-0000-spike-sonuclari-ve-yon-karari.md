# ADR-0000: Spike Sonuclari ve Yon Karari

Tarih: 2026-06-07

## Durum

Onceki spike'lar ve dokuman degerlendirmeleri su tabloyu olusturdu:

| Alan | Kanitlanan | Kanitlanmayan |
| --- | --- | --- |
| Acp.Net | .NET 8 client, stdio, Python process ve JSON-RPC akisinin calisabildigi | Mevcut AgentClientProtocol paketi karsisinda net fark |
| Platform interop | Windows/WSL path, process lifecycle ve runtime dogrulamasinin ana zorluk oldugu | Tum edge-case'lerin SDK seviyesinde cozulebilecegi |
| Training Factory | Oyuncak RL ortaminda rollout yapilabildigi | SB3 egitiminin baseline'i gecebildigi, Gazebo/PX4 stabilitesi, reproducibility |
| Ajan verimliligi | Kod uretiminin hizli, dogrulamanin yavas oldugu | Ajanla gelistirmenin deterministik hizlandirici oldugu |

## Karar

Acp.Net icin calisma devam etmeli, ama sadece incumbent karsilastirmadan sonra urunlesme karari verilmeli.

Training Factory simdilik ana urun olarak ele alinmamali. Acp.Net'in ileride kullanabilecegi test/dogfooding alani olarak raflanmali. Training Factory icin bir sonraki deney random rollout degil, gercek SB3 learning baseline olmalidir.

## Gerekce

Spike 1, Acp.Net'in deger alaninin protokol wrapper'i yazmak degil, platformun kirli gerceklerini SDK/API seviyesinde temizlemek oldugunu gosterdi. Ozellikle process lifecycle, Windows/WSL yol cevirisi, stdio bridge, cancel/timeout ve test helper alanlari fark yaratabilir.

Spike 2, Training Factory icin yeterli degil. Random ajanla 20 step calismak ortam kurulabildigini gosterir, fakat "urun degeri var" sonucunu tasimaz. Deger icin egitim sinyali, eval suite, baseline karsilastirmasi ve tekrarlanabilirlik gerekir.

Spike 3, ajanin hizlandirici etkisinin dogrulama darboğazini ortadan kaldirmadigini gosterdi. Bu nedenle zaman planlari kod yazma hizina degil, test ve ortam dogrulama maliyetine gore yapilmali.

## Sonuc

Bir sonraki kritik karar kapisi AgentClientProtocol incumbent karsilastirmasidir. Bu testten once Acp.Net'e tam commit edilmemeli. Test sonucunda fark netse Acp.Net dar kapsamli MVP'ye iner; fark zayifsa proje yeniden konumlandirilir veya durdurulur.

## Kabul edilen zaman bakisi

- Acp.Net incumbent karsilastirma: 1 gun.
- Acp.Net MVP kapsam/prototype: 1-3 hafta, sadece fark alanlarina odaklanirsa.
- Training Factory gercek spike: 2-4 hafta.
- Training Factory urunlesme: en az 6-12 ay ve yuksek entegrasyon riski.
