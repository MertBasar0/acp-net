# 2026-06-07 Sonraki Aksiyon Plani

Bu plan, spike sonuclari ve dokuman degerlendirmelerinden sonra izlenecek sirayi netlestirir. Ana ilke: Acp.Net icin incumbent karsilastirma bitmeden urunlesme varsayimi yapilmayacak; Training Factory ise ogrenme sinyali kanitlanmadan ana urun gibi ele alinmayacak.

## Oncelik 1: AgentClientProtocol Incumbent Karsilastirmasi

Durum: Tamamlandi.

Amaç:

Mevcut `AgentClientProtocol` NuGet paketinin Acp.Net'in iddia ettigi deger alanlarini zaten cozup cozmedigini gormek.

Yapilacaklar:

1. `src/spikes/acp-incumbent-comparison/` altinda iki minimal .NET sample hazirla.
2. Ilk sample mevcut `AgentClientProtocol` paketini kullansin.
3. Ikinci sample spike'ta dogrulanan Acp.Net yaklasimini temsil etsin.
4. Ayni mock ACP server ile su akislar kosulsun:
   - `initialize`
   - prompt request-response
   - streaming response
   - cancel/timeout
   - child process lifecycle
   - Windows/WSL path ve runtime bridge
5. Her kosu icin transcript, hata mesaji ve gereken call-site kod miktari kaydedilsin.
6. `docs/spikes/001-agentclientprotocol-incumbent-comparison.md` icindeki skor tablosu doldurulsun.

Kabul kriteri:

Bu spike'in sonucu calisan demo degil, karar olmalidir:

- `go`: Acp.Net mevcut pakete gore net fark yaratiyor.
- `narrow`: Fark var ama sadece process lifecycle / Windows-WSL interop gibi dar alanda.
- `no-go`: Mevcut paket yeterli; Acp.Net ayri urun olmamali.

Zaman kutusu: 1 gun.

Sonuc:

`narrow`. Mevcut `AgentClientProtocol` paketi protokol/schema tarafinda yeterli. Acp.Net tam SDK degil, process/runtime/testing helper olarak daraltilmali.

## Oncelik 2: Acp.Net MVP Kapsam Karari

Durum: Tamamlandi.

Amaç:

Acp.Net'in ayri bir proje olarak devam edip etmeyecegini ve devam edecekse ilk MVP'nin ne kadar dar olacagini belirlemek.

MVP'ye girmesi muhtemel alanlar:

- Process lifecycle wrapper.
- Windows/WSL path normalization.
- Stdio JSON-RPC transcript/debug helper.
- Timeout/cancel davranisi.
- Test helper ve fake ACP server.

MVP'ye simdilik girmemesi gereken alanlar:

- Tam protokol surface'ini bastan yazmak.
- UI.
- Training Factory entegrasyonu.
- Coklu provider/plugin marketplace.

Kabul kriteri:

`docs/decisions/ADR-0001-acp-net-incumbent-karari.md` dosyasi yazilsin ve `go / narrow / no-go` karari tek cumleyle ozetlensin.

Sonuc:

Karar kaydi yazildi. Urun tasarimi `docs/product/acp-net-mvp-product-design.md` altinda.

## Oncelik 2.5: Acp.Net Process/Testing MVP Spike

Durum: Siradaki teknik is.

Amaç:

Daraltilmis Acp.Net MVP'nin gercek kodla kullanilabilir olup olmadigini test etmek.

Yapilacaklar:

1. `src/acp-net/` altinda minimal .NET solution olustur.
2. `Acp.Net.Process` ve `Acp.Net.Testing` projelerini ekle.
3. `AcpProcessRunner` ile process lifecycle, WSL bridge ve transcript davranisini kapsa.
4. `AgentClientProtocol` paketini kullanan sample yaz.
5. Fake ACP agent ile integration test ekle.

Kabul kriteri:

`docs/spikes/003-acp-net-process-testing-mvp.md` icindeki kabul kriterleri saglanmali.

## Oncelik 3: Training Factory SB3 Baseline

Durum: Acp.Net kararindan sonra veya paralel denenecekse dusuk oncelikle.

Amaç:

Training Factory icin "ortam calisiyor" yerine "ajan ogreniyor mu" sorusuna cevap vermek.

Yapilacaklar:

1. `src/spikes/training-factory-sb3-baseline/` altinda tekrarlanabilir deney klasoru olustur.
2. Hafif bir RL/drone ortami sec.
3. Random baseline'i sabit seed ile olc.
4. SB3 policy egit.
5. Train ve eval episode'larini ayir.
6. Sonuclari metrik olarak kaydet:
   - random baseline mean reward
   - trained policy mean reward
   - episode sayisi
   - seed
   - paket surumleri
   - egitim suresi

Kabul kriteri:

SB3 policy random baseline'i anlamli sekilde gecerse Training Factory ikinci hat olarak tutulur. Gecemezse urun hipotezi rafa kaldirilir ve sadece Acp.Net dogfooding senaryosu olarak saklanir.

Zaman kutusu: 2-4 hafta.

## Oncelik 4: Dokumantasyon Disiplini

Bundan sonraki her calisma openclaw workspace altindaki bu klasorde tutulacak:

`/home/mertb/.openclaw/workspace/acp-net-training-factory`

Beklenen yapi:

- `docs/decisions/`: karar kayitlari.
- `docs/spikes/`: spike planlari ve sonuc raporlari.
- `docs/plans/`: uygulanabilir aksiyon planlari.
- `docs/ops/`: operasyonel notlar.
- `src/spikes/`: deney kodlari.

Her spike tamamlandiginda ayni klasorde en az su iki cikti olmalidir:

- Calistirilabilir kod veya komut dosyasi.
- Kisa sonuc raporu ve karar etkisi.
