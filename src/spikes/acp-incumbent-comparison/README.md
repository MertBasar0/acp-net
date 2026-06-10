# ACP Incumbent Comparison Spike

🇹🇷 Türkçe sürüm: [README.tr.md](README.tr.md)

This folder compares the `AgentClientProtocol` NuGet package with the platform/process helper approach considered for Acp.Net, against the same mock ACP agent. The decision distilled from this comparison is recorded in [ADR-0001](../../../docs/decisions/ADR-0001-incumbent-comparison-decision.md).

## Files

- `AcpIncumbentComparison.csproj`: .NET 8 console spike project.
- `Program.cs`: the two comparison modes.
- `mock_acp_agent.py`: stdio JSON-RPC mock ACP agent.

## Modes

- `incumbent`: uses the `ClientSideConnection` API of the `AgentClientProtocol` package.
- `acpnet`: uses the raw JSON-RPC + process bridge wrapper approach.
- `both`: runs both modes back to back.

## Run

From the repository root:

```bash
dotnet run --project src/spikes/acp-incumbent-comparison/AcpIncumbentComparison.csproj -- both
```

On Windows + WSL setups see the path note in [docs/DEVELOPMENT_GUIDE.md](../../../docs/DEVELOPMENT_GUIDE.md).

## Expected Output

- `incumbent-transcript.ndjson`
- `acpnet-style-transcript.ndjson`
- a score summary in the terminal
