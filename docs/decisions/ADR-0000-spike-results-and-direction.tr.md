# ADR-0000: Spike Sonuçları ve Yön Kararı

> 🇬🇧 English version: [ADR-0000-spike-results-and-direction.md](ADR-0000-spike-results-and-direction.md)

Tarih: 2026-06-07

## Durum

Önceki spike'lar ve doküman değerlendirmeleri şu tabloyu oluşturdu:

| Alan | Kanıtlanan | Kanıtlanmayan |
| --- | --- | --- |
| Acp.Net | .NET 8 client, stdio, Python process ve JSON-RPC akışının çalışabildiği | Mevcut AgentClientProtocol paketi karşısında net fark |
| Platform interop | Windows/WSL path, process yaşam döngüsü ve runtime doğrulamasının ana zorluk olduğu | Tüm edge-case'lerin SDK seviyesinde çözülebileceği |
| Training Factory | Oyuncak RL ortamında rollout yapılabildiği | SB3 eğitiminin baseline'ı geçebildiği, Gazebo/PX4 stabilitesi, tekrarlanabilirlik |
| Ajan verimliliği | Kod üretiminin hızlı, doğrulamanın yavaş olduğu | Ajanla geliştirmenin deterministik hızlandırıcı olduğu |

## Karar

Acp.Net için çalışma devam etmeli, ama ürünleşme kararı yalnızca incumbent karşılaştırmasından sonra verilmeli.

Training Factory şimdilik ana ürün olarak ele alınmamalı. Acp.Net'in ileride kullanabileceği test/dogfooding alanı olarak raflanmalı. Training Factory için bir sonraki deney random rollout değil, gerçek SB3 learning baseline olmalıdır.

## Gerekçe

Spike 1, Acp.Net'in değer alanının protokol wrapper'ı yazmak değil, platformun kirli gerçeklerini SDK/API seviyesinde temizlemek olduğunu gösterdi. Özellikle process yaşam döngüsü, Windows/WSL yol çevirisi, stdio köprüsü, cancel/timeout ve test yardımcısı alanları fark yaratabilir.

Spike 2, Training Factory için yeterli değil. Random ajanla 20 adım çalışmak ortamın kurulabildiğini gösterir, fakat "ürün değeri var" sonucunu taşımaz. Değer için eğitim sinyali, eval suite, baseline karşılaştırması ve tekrarlanabilirlik gerekir.

Spike 3, ajanın hızlandırıcı etkisinin doğrulama darboğazını ortadan kaldırmadığını gösterdi. Bu nedenle zaman planları kod yazma hızına değil, test ve ortam doğrulama maliyetine göre yapılmalı.

## Sonuç

Bir sonraki kritik karar kapısı AgentClientProtocol incumbent karşılaştırmasıdır. Bu testten önce Acp.Net'e tam commit edilmemeli. Test sonucunda fark netse Acp.Net dar kapsamlı MVP'ye iner; fark zayıfsa proje yeniden konumlandırılır veya durdurulur.

## Kabul Edilen Zaman Bakışı

- Acp.Net incumbent karşılaştırması: 1 gün.
- Acp.Net MVP kapsam/prototip: 1–3 hafta, sadece fark alanlarına odaklanırsa.
- Training Factory gerçek spike: 2–4 hafta.
- Training Factory ürünleşmesi: en az 6–12 ay ve yüksek entegrasyon riski.

## Kanıt

Bu ADR'nin arkasındaki spike oturum raporları (spike 001–003), deponun dışında bakımcının yerel mühendislik notları olarak tutulmaktadır.
