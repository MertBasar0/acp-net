# Spike 003 Sonuc Raporu: Acp.Net Process/Testing MVP

Tarih: 2026-06-07

## Ozet

Spike basarili.

Daraltilmis Acp.Net MVP icin ilk calisan kod uretildi:

- `Acp.Net.Process`
- `Acp.Net.Testing`
- `AgentClientProtocol` ile calisan sample
- console tabanli integration test runner

Bu sonuc, ADR-0001'deki `narrow` kararini teknik olarak destekliyor: Acp.Net'in degeri protokol SDK'sini yeniden yazmakta degil, process/runtime/testing katmaninda.

## Uretilen Kod

Process paketi:

`src/acp-net/Acp.Net.Process/`

Ana tipler:

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpProcessSession`
- `AcpRuntime`
- `AcpRuntimeResolver`
- `AcpPathMapper`
- `AcpShutdownPolicy`
- `AcpTranscriptRecorder`
- `RecordingTextReader`
- `RecordingTextWriter`

Testing paketi:

`src/acp-net/Acp.Net.Testing/`

Ana tipler:

- `FakeAcpAgentScript`
- `AcpTranscriptAssert`

Sample:

`src/samples/acp-process-with-agentclientprotocol/`

Integration runner:

`src/acp-net/Acp.Net.IntegrationTests/`

## Dogrulama

Integration test runner:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\acp-net\Acp.Net.IntegrationTests\Acp.Net.IntegrationTests.csproj'
```

Cikti:

```text
ok - agentclientprotocol sample flow
ok - hard kill fallback
PASSED
```

Sample:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\acp-process-with-agentclientprotocol\acp-process-with-agentclientprotocol.csproj'
```

Cikti:

```text
hello worldprotocol=1
session=fake-session-1
stopReason=EndTurn
usesWsl=True
transcript=...\sample-transcript.ndjson
```

## Kanitlananlar

1. `AgentClientProtocol` ile beraber calisan runner yazilabildi.
2. Windows `dotnet.exe` icinden WSL `python3` fake ACP agent calistirildi.
3. WSL runtime karari uygulama kodunda degil `AcpRuntimeResolver` icinde verildi.
4. UNC path -> WSL path donusumu runner tarafinda yapildi.
5. stdin/stdout/stderr transcript kaydi uretildi.
6. `session/update` streaming akisi typed `AgentClientProtocol` client'a ulasti.
7. Hanging agent senaryosunda hard kill fallback calisti.

## Kalan Eksikler

Bu henuz urun MVP'si degil, teknik spike.

Eksikler:

- Public API isimleri henuz dondurulmedi.
- xUnit/NUnit gibi standart test framework entegrasyonu yok.
- NuGet metadata/package build yok.
- Daha fazla runtime kombinasyonu test edilmedi.
- Graceful shutdown icin agent'a explicit protocol cancel/exit stratejisi henuz yok; mevcut davranis stdin close + kill fallback.
- Transcript assertion helper minimal.

## Karar Etkisi

Karar: Acp.Net Process/Testing MVP yoluna devam edilebilir.

Not: 2026-06-07 devam adiminda bu prototip xUnit `dotnet test`, `.slnx` solution ve `dotnet pack` dogrulamasina tasindi. Detay:

`docs/spikes/004-acp-net-api-test-packaging-hardening-result.md`

Bir sonraki adim:

1. API yuzeyini kucult ve isimleri netlestir.
2. `Acp.Net.Process` icin unit/integration testleri ayir.
3. Standard test framework sec.
4. Paketleme/NuGet hazirliklarini ekle.
5. README seviyesinde ilk kullanim dokumani yaz.
