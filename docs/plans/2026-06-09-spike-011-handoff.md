# 2026-06-09 Spike 011 Handoff

## Bugunku Ek Calisma

Spike 011 tamamlandi:

**OpenClaw Plugin/Command Integration Probe**

## Eklenenler

C# probe:

`src/samples/openclaw-acpnet-probe/`

Node wrapper probe:

`src/openclaw-probe/`

## Son Dogrulanan Komutlar

C# probe:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\openclaw-acpnet-probe\openclaw-acpnet-probe.csproj'
```

Sonuc:

```text
kind=openclaw.acpnet.probe.result
ok=true
result=completed
failureKind=None
sessionId=fake-session-1
stopReason=EndTurn
```

Ana test suite:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

Sonuc:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
```

## Onemli Bulgu

Node child process icinden `dotnet` / Windows interop cagrisi bu Codex sandbox'ta asagidaki hata ile dusuyor:

```text
WSL ERROR: UtilBindVsockAnyPort:307: socket failed 1
```

Ayni `dotnet run` shell'den dogrudan calisiyor.

Bu, OpenClaw entegrasyonunda Node runtime -> Windows interop boundary'sinin ayri dogrulanmasi gerektigini gosteriyor.

## Sonraki Onerilen Is

Spike 012:

**OpenClaw ACPX Contract Comparison**

OpenClaw'daki `extensions/acpx` su anda en ilgili parca. Acp.Net'i dogrudan OpenClaw core'a eklemeden once ACPX runtime/lease/event contract'i ile Acp.Net run artifact/failure contract'i yan yana incelenmeli.

