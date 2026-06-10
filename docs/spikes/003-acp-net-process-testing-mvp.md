# Spike 003: Acp.Net Process/Testing MVP

## Amac

Durum: 2026-06-07 ilk implementasyon spike'i tamamlandi.

Sonuc raporu:

`docs/spikes/003-acp-net-process-testing-mvp-result.md`

ADR-0001 sonrasi daraltilan Acp.Net kapsaminin gercek bir MVP API'sine donusup donusemeyecegini test etmek.

Bu spike'in sorusu:

> `AgentClientProtocol` paketini yeniden yazmadan, onu tamamlayan process/runtime/testing katmani anlamli sekilde kullanilabilir mi?

## Kapsam

Kod yeri:

`src/acp-net/`

Ilk hedef paketler:

- `Acp.Net.Process`
- `Acp.Net.Testing`

Ilk sample:

`src/samples/acp-process-with-agentclientprotocol/`

## Yapilacaklar

1. Minimal .NET solution olustur.
2. `Acp.Net.Process` icinde su tipleri tasarla:
   - `AcpProcessRunner`
   - `AcpProcessOptions`
   - `AcpProcessSession`
   - `AcpRuntime`
   - `AcpShutdownPolicy`
   - `AcpTranscriptRecorder`
3. Windows/WSL runtime resolver ekle:
   - Windows process icinden WSL agent calistirma.
   - Linux/WSL icinden native `python3` calistirma.
   - UNC path -> WSL path donusumu.
4. `Acp.Net.Testing` icinde fake ACP agent fixture tasarla.
5. `AgentClientProtocol` paketiyle calisan sample yaz.
6. Integration test ekle:
   - initialize
   - session/new
   - session/prompt streaming
   - cancel
   - graceful shutdown
   - hard kill fallback
   - transcript assertion

## Kapsam Disi

- ACP schema modeli yazmak.
- Custom typed client yazmak.
- UI.
- Training Factory.
- NuGet publish.
- Production daemon.

## Kabul Kriterleri

Spike tamamlanmis sayilmasi icin:

1. `dotnet test` veya net bir test komutu ile integration testler gecmeli.
2. Sample, `AgentClientProtocol` ile beraber runner'i kullanarak mock ACP agent'a baglanmali.
3. Transcript dosyasi uretmeli.
4. Windows/WSL path/runtime karari uygulama kodunda degil runner icinde olmali.
5. Fail eden agent senaryosunda process hard kill ile temizlenmeli.
6. Sonuc raporu `docs/spikes/003-acp-net-process-testing-mvp-result.md` olarak yazilmali.

## Karar Etkisi

Sonuc: Basarili. Acp.Net icin ilk gercek MVP yoluna girilebilir.

Basarisiz olursa iki ihtimal kalir:

- mevcut `AgentClientProtocol` paketine upstream katkisi.
- Acp.Net'i sadece dokumante edilmis integration recipe olarak birakmak.

## Zaman Kutusu

2-3 gunluk teknik spike.

Bu surede process runner + fake agent testleri calismazsa kapsam daha da daraltilmali.
