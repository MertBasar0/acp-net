# ADR-0001: Acp.Net Incumbent Karsilastirma Karari

Tarih: 2026-06-07

## Durum

`AgentClientProtocol` NuGet paketi ile Acp.Net-style process bridge yaklasimi ayni mock ACP agent uzerinde karsilastirildi.

Kullanilan paket:

- `AgentClientProtocol` 0.1.5
- NuGet: https://www.nuget.org/packages/AgentClientProtocol
- Kaynak repo: https://github.com/nuskey8/acp-csharp

Calisan spike:

`src/spikes/acp-incumbent-comparison/`

Sonuc raporu:

`docs/spikes/001-agentclientprotocol-incumbent-comparison-result-2026-06-07.md`

## Karar

Karar: `narrow`.

Acp.Net, `AgentClientProtocol` paketine rakip olacak tam protokol SDK'si olarak konumlandirilmamali.

Acp.Net'in ilk deger alani su sekilde daraltilmali:

- stdio process lifecycle
- Windows/WSL path ve runtime bridge
- stdout/stderr ayrimi
- timeout, graceful stop, hard kill
- raw transcript/debug helper
- fake ACP server ve test assertion helper

## Gerekce

`AgentClientProtocol` paketi typed protocol/schema tarafinda iyi calisti:

- `initialize`
- `session/new`
- `session/prompt`
- streaming `session/update`
- `session/cancel`

Bu yuzeyi yeniden yazmak bakim maliyetini hakli cikarmiyor.

Ancak paketin kendi orneklerinde ve calistirilan spike'ta su konular uygulama koduna kaldi:

- agent process baslatma
- stderr drain
- process kapatma stratejisi
- Windows `dotnet.exe` icinden WSL `python3` agent'a gecis
- UNC path / WSL path donusumu
- transcript/debug kaydi
- test harness ergonomisi

Onceki spike'larda da en cok zaman bu alanlara gitmisti. Bu nedenle deger protokol modelinde degil, platform/process katmaninda.

## Sonuc

Bir sonraki teknik is Acp.Net MVP'sini dar kapsamla tasarlamak:

- `Acp.Net.Process`
- `Acp.Net.Testing`
- mevcut `AgentClientProtocol` paketiyle birlikte calisabilen stdio harness

Training Factory bu kararin disinda kalir. Simdilik Acp.Net sonrasi dogfooding veya ikinci hat spike olarak tutulur.
