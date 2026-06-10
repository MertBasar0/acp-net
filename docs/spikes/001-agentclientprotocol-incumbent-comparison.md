# Spike 001: AgentClientProtocol Incumbent Karsilastirmasi

## Amac

Mevcut AgentClientProtocol NuGet paketinin, Acp.Net fikrinin hedefledigi deger alanlarini zaten cozup cozmedigini test etmek.

Bu spike'in cevabi: "Acp.Net yazmaya deger mi, yoksa mevcut paketin uzerine katkida bulunmak daha mantikli mi?"

## Kapsam

Karsilastirma ayni mock ACP server senaryosu uzerinden yapilacak:

- `initialize` / capability negotiation.
- Basit prompt request-response.
- Streaming response.
- Cancel/timeout davranisi.
- Process lifecycle: child process baslatma, stdout/stderr ayirma, graceful kill, hard kill.
- Windows host + WSL bridge senaryosu.
- Path handling: Windows path, WSL path, UNC path.
- Test helper ergonomisi.

## Karsilastirma matrisi

Durum: 2026-06-07 ilk calisan karsilastirma tamamlandi.

Kaynaklar:

- NuGet: https://www.nuget.org/packages/AgentClientProtocol
- Kaynak repo: https://github.com/nuskey8/acp-csharp

| Kriter | AgentClientProtocol | Acp.Net spike yaklasimi | Not |
| --- | --- | --- | --- |
| Kurulum kolayligi | 2 | 1 | NuGet paketi net8.0 ile calisti; Acp.Net-style bridge su an spike kodu |
| Protocol coverage | 2 | 1 | Paket typed schema/API sagliyor; bridge raw JSON kullaniyor |
| Streaming | 2 | 1 | Paket `session/update` notification'larini typed deserialize etti |
| Cancellation | 1 | 1 | Paket `CancelAsync` sagliyor; process timeout/kill uygulamaya kaliyor |
| Process lifecycle | 1 | 2 | Child process start, stderr drain, kill pakette yok; bridge tek yerde topladi |
| Windows/WSL interop | 0 | 2 | Paket path/runtime bridge saglamiyor; bridge `wsl.exe python3` kararini kapsadi |
| Test helper | 1 | 2 | Paket mock/transcript helper vermiyor; bridge transcript uretti |
| Debuggability | 1 | 2 | Paket icin ekstra `TextWriter` wrapper gerekti; bridge raw in/out transcript tuttu |
| API ergonomisi | 2 | 1 | Paket call-site'i typed ve temiz; bridge platformda iyi ama protokolde ham |

Skor: 0 = yok/zayif, 1 = var ama pürüzlü, 2 = yeterli/iyi.

## Kabul kriterleri

Spike basarili sayilmaz; karar uretirse tamamlanmis sayilir.

Devam karari icin en az iki kosul lazim:

1. AgentClientProtocol paketi process lifecycle veya Windows/WSL interop alaninda belirgin bosluk birakiyor.
2. Acp.Net yaklasimi bu boslugu daha az kodla veya daha guvenilir test edilebilir sekilde kapatabiliyor.

Dur karari icin su kosullardan biri yeterli:

1. Mevcut paket ayni senaryolari temiz API ile cozuyor.
2. Fark sadece isimlendirme/ergonomi seviyesinde kaliyor.
3. Acp.Net'in farki, bakim maliyetini hakli cikarmiyor.

## Beklenen cikti

- Calisan iki minimal sample.
- Kisa log/transcript kaniti.
- Skor tablosu.
- Go / no-go / daralt karar notu.

## Ilk sonuc

Karar: `narrow`.

`AgentClientProtocol` mevcut haliyle protokol/schema katmaninda yeterince iyi sinyal verdi. Bu nedenle Acp.Net'in tam protokol SDK'si olarak konumlanmasi simdilik zayif.

Acp.Net icin anlamli kalan alan daha dar:

- process lifecycle wrapper
- Windows/WSL path ve runtime bridge
- stdout/stderr ayrimi
- timeout/graceful kill/hard kill
- raw transcript/debug kaydi
- mock ACP server ve test assertion helper

Calisan spike kodu:

`src/spikes/acp-incumbent-comparison/`

Dogulama komutu:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\spikes\acp-incumbent-comparison\AcpIncumbentComparison.csproj' -- both
```

Ozet cikti:

```text
AgentClientProtocol: protocol=2, streaming=2, lifecycle=1, interop=0, debug=1
Acp.Net-style bridge: protocol=1, streaming=1, lifecycle=2, interop=2, debug=2
```

## Zaman kutusu

1 gun. Bu surede net karar cikmiyorsa Acp.Net riski yuksek kabul edilmeli ve kapsam daha da daraltilmali.
