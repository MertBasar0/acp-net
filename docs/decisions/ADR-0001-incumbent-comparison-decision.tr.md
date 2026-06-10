# ADR-0001: Acp.Net Incumbent Karşılaştırma Kararı

> 🇬🇧 English version: [ADR-0001-incumbent-comparison-decision.md](ADR-0001-incumbent-comparison-decision.md)

Tarih: 2026-06-07

## Durum

`AgentClientProtocol` NuGet paketi ile Acp.Net tarzı process bridge yaklaşımı aynı mock ACP agent üzerinde karşılaştırıldı.

Kullanılan paket:

- `AgentClientProtocol` 0.1.5
- NuGet: https://www.nuget.org/packages/AgentClientProtocol
- Kaynak repo: https://github.com/nuskey8/acp-csharp

Çalışan spike kodu:

`src/spikes/acp-incumbent-comparison/`

## Karar

Karar: `narrow` (daralt).

Acp.Net, `AgentClientProtocol` paketine rakip olacak tam protokol SDK'sı olarak konumlandırılmamalı.

Acp.Net'in ilk değer alanı şu şekilde daraltılmalı:

- stdio process yaşam döngüsü
- Windows/WSL path ve runtime köprüsü
- stdout/stderr ayrımı
- timeout, nazik durdurma (graceful stop), zorla sonlandırma (hard kill)
- ham transcript/debug yardımcıları
- sahte ACP server ve test doğrulama yardımcıları

## Gerekçe

`AgentClientProtocol` paketi typed protokol/şema tarafında iyi çalıştı:

- `initialize`
- `session/new`
- `session/prompt`
- streaming `session/update`
- `session/cancel`

Bu yüzeyi yeniden yazmak bakım maliyetini haklı çıkarmıyor.

Ancak paketin kendi örneklerinde ve çalıştırılan spike'ta şu konular uygulama koduna kaldı:

- agent process'ini başlatma
- stderr drain
- process kapatma stratejisi
- Windows `dotnet.exe` içinden WSL `python3` agent'a geçiş
- UNC path / WSL path dönüşümü
- transcript/debug kaydı
- test harness ergonomisi

Önceki spike'larda da en çok zaman bu alanlara gitmişti. Bu nedenle değer protokol modelinde değil, platform/process katmanında.

## Sonuç

Bir sonraki teknik iş, Acp.Net MVP'sini dar kapsamla tasarlamak:

- `Acp.Net.Process`
- `Acp.Net.Testing`
- mevcut `AgentClientProtocol` paketiyle birlikte çalışabilen stdio harness

Training Factory bu kararın dışında kalır. Şimdilik Acp.Net sonrası dogfooding veya ikinci hat spike olarak tutulur.

## Kanıt

Karşılaştırma skor tablosu ve çalıştırma transcript'leri, depo kökündeki, git tarafından takip edilmeyen `notes/` klasöründe tutulan spike oturum raporlarında (spike 001) kayıtlıdır. Çalıştırılabilir karşılaştırma kodu `src/spikes/acp-incumbent-comparison/` altında durmaktadır.
