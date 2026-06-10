# ACP Incumbent Comparison Spike

Bu klasor, `AgentClientProtocol` NuGet paketi ile Acp.Net icin dusunulen platform/process helper yaklasimini ayni mock ACP agent uzerinde karsilastirir.

## Dosyalar

- `AcpIncumbentComparison.csproj`: .NET 8 console spike projesi.
- `Program.cs`: iki karsilastirma modu.
- `mock_acp_agent.py`: stdio JSON-RPC mock ACP agent.

## Modlar

- `incumbent`: `AgentClientProtocol` paketinin `ClientSideConnection` API'sini kullanir.
- `acpnet`: raw JSON-RPC + process bridge wrapper yaklasimini kullanir.
- `both`: iki modu arka arkaya calistirir.

## Beklenen komut

Bu makinede WSL icinden Windows `dotnet.exe` kullanildigi icin proje yolu UNC olarak verilmelidir:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\spikes\acp-incumbent-comparison\AcpIncumbentComparison.csproj' -- both
```

## Beklenen cikti

- `incumbent-transcript.ndjson`
- `acpnet-style-transcript.ndjson`
- terminalde score summary
