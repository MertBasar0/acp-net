# ACP Incumbent Karşılaştırma Spike'ı

🇬🇧 English version: [README.md](README.md)

Bu klasör, `AgentClientProtocol` NuGet paketi ile Acp.Net için düşünülen platform/process yardımcısı yaklaşımını aynı mock ACP agent üzerinde karşılaştırır. Bu karşılaştırmadan damıtılan karar [ADR-0001](../../../docs/decisions/ADR-0001-incumbent-comparison-decision.tr.md) içinde kayıtlıdır.

## Dosyalar

- `AcpIncumbentComparison.csproj`: .NET 8 console spike projesi.
- `Program.cs`: iki karşılaştırma modu.
- `mock_acp_agent.py`: stdio JSON-RPC mock ACP agent.

## Modlar

- `incumbent`: `AgentClientProtocol` paketinin `ClientSideConnection` API'sini kullanır.
- `acpnet`: ham JSON-RPC + process bridge wrapper yaklaşımını kullanır.
- `both`: iki modu arka arkaya çalıştırır.

## Çalıştırma

Depo kökünden:

```bash
dotnet run --project src/spikes/acp-incumbent-comparison/AcpIncumbentComparison.csproj -- both
```

Windows + WSL kurulumları için [docs/DEVELOPMENT_GUIDE.tr.md](../../../docs/DEVELOPMENT_GUIDE.tr.md) içindeki path notuna bakın.

## Beklenen Çıktı

- `incumbent-transcript.ndjson`
- `acpnet-style-transcript.ndjson`
- terminalde bir skor özeti
