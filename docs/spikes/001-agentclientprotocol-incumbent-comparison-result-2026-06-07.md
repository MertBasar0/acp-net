# Spike 001 Sonuc Raporu: AgentClientProtocol Incumbent Karsilastirmasi

Tarih: 2026-06-07

## Ozet

Spike calistirildi ve karar uretti.

Karar: `narrow`.

`AgentClientProtocol` NuGet paketi protokol/schema/typed API tarafinda yeterli sinyal verdi. Acp.Net'i ayni yuzeyi bastan yazan tam bir SDK olarak konumlamak su an dogru gorunmuyor.

Acp.Net icin degerli kalan alan daha dar ve daha net:

- stdio child process lifecycle
- Windows/WSL runtime bridge
- path normalization
- stdout/stderr ayrimi
- timeout, graceful stop, hard kill
- raw transcript/debug helper
- mock ACP server ve test helper

## Kaynak Dogrulamasi

NuGet paketi:

- `AgentClientProtocol`
- Surum: `0.1.5`
- Hedef framework: `.NET 8.0`, `.NET Standard 2.1`
- Kaynak: https://www.nuget.org/packages/AgentClientProtocol

Kaynak repo:

- https://github.com/nuskey8/acp-csharp

Repo incelemesinde paketin `ClientSideConnection`, `IAcpClient`, `IAcpAgent` ve schema tiplerini sagladigi goruldu. Child process baslatma ve process lifecycle davranisi ornek uygulamada uygulama koduna birakilmis.

## Uretilen Kod

Klasor:

`src/spikes/acp-incumbent-comparison/`

Dosyalar:

- `AcpIncumbentComparison.csproj`
- `Program.cs`
- `mock_acp_agent.py`
- `README.md`
- `.gitignore`

## Calistirilan Komut

Bu makinede WSL icinden Windows `dotnet.exe` kullanildigi icin proje UNC path ile calistirildi:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\spikes\acp-incumbent-comparison\AcpIncumbentComparison.csproj' -- both
```

## Cikti

```text
=== score-summary ===
AgentClientProtocol: protocol=2, streaming=2, lifecycle=1, interop=0, debug=1
- NuGet SDK protocol/schema tarafini calistirdi.
- Child process baslatma, stderr drain, kill ve path karari uygulama kodunda kaldi.
- Transcript icin TextWriter wrapper gibi ek uygulama kodu gerekti.
Acp.Net-style bridge: protocol=1, streaming=1, lifecycle=2, interop=2, debug=2
- Protocol typing zayif; raw JSON ile calisiyor.
- Process lifecycle, WSL bridge ve transcript tek yerde toplandi.
- Bu yaklasim SDK degil, mevcut SDK'nin altinda/yaninda degerli olacak platform helper sinyalini veriyor.
```

## Kanit Dosyalari

Run sonucunda su transcript dosyalari uretildi:

- `src/spikes/acp-incumbent-comparison/incumbent-transcript.ndjson`
- `src/spikes/acp-incumbent-comparison/acpnet-style-transcript.ndjson`

Bu dosyalar `.gitignore` kapsaminda tutuldu; tekrar kosuda yeniden uretilebilir.

## Bulgu

`AgentClientProtocol` paketi typed protocol client/agent API'si icin yeterli. `initialize`, `session/new`, `session/prompt`, streaming `session/update` ve `session/cancel` akislari mock ACP agent ile calisti.

Ancak paket su alanlari cozmedi:

- child process baslatma
- process kapanis stratejisi
- stderr drain
- Windows processinden WSL icindeki Python agent'a gecis
- UNC path / WSL path karari
- raw transcript kaydi
- fake server/test ergonomisi

Bu alanlar Acp.Net icin gercek ve daha savunulabilir deger alani.

## Karar Etkisi

Acp.Net icin bir sonraki karar tam SDK degil, dar helper/MVP olmalidir.

Onerilen yeni kapsam:

- `Acp.Net.Process`
- `Acp.Net.Testing`
- mevcut `AgentClientProtocol` paketiyle entegre olabilen process/stdio harness

Training Factory bu karardan etkilenmez; hala Acp.Net sonrasi veya dogfooding alani olarak tutulmali.
