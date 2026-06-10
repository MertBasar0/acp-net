# Spike 005 Sonuc Raporu: API Surface, Docs ve Unit Tests

Tarih: 2026-06-07

## Ozet

Spike basarili.

Acp.Net Process/Testing prototipi alpha NuGet oncesi daha temiz bir API yuzeyine ve standart test yapisina tasindi.

## Yapilanlar

- Public C# namespace'leri `AcpNet.Process` ve `AcpNet.Testing` olarak korundu.
- Public API yuzeyi daraltildi.
- Internal helper'lar `internal` yapildi.
- Unit test projesi eklendi.
- Paket README'leri gercek kullanim ornekleriyle genisletildi.
- `dotnet test`, sample run ve `dotnet pack` dogrulandi.

## Public API

`Acp.Net.Process` icin public kalmasi hedeflenen yuzey:

- `AcpProcessRunner`
- `AcpProcessOptions`
- `AcpProcessSession`
- `AcpRuntime`
- `AcpShutdownPolicy`
- `AcpTranscriptRecorder`

Internal hale getirilen helper'lar:

- `AcpRuntimeResolver`
- `AcpPathMapper`
- `AcpProcessStartInfo`
- `RecordingTextReader`
- `RecordingTextWriter`

`Acp.Net.Testing` public yuzeyi:

- `FakeAcpAgentScript`
- `AcpTranscriptAssert`

## Eklenen Testler

Unit test projesi:

`src/acp-net/Acp.Net.UnitTests/`

Kapsam:

- WSL/UNC path mapping.
- Windows drive path mapping.
- runtime resolver native/WSL command olusturma.
- transcript recorder JSON/plain text kaydi.
- recording reader/writer davranisi.

Integration testler korunup xUnit solution icinde calismaya devam etti.

## Dogrulama

Solution test:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=normal'
```

Cikti ozeti:

```text
Acp.Net.UnitTests: 9 passed
Acp.Net.IntegrationTests: 2 passed
```

Sample:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\acp-process-with-agentclientprotocol\acp-process-with-agentclientprotocol.csproj'
```

Cikti ozeti:

```text
hello worldprotocol=1
session=fake-session-1
stopReason=EndTurn
usesWsl=True
```

Pack:

```bash
dotnet pack ...Acp.Net.Process.csproj --no-restore --output ...artifacts\packages
dotnet pack ...Acp.Net.Testing.csproj --no-restore --output ...artifacts\packages
```

Uretilen paketler:

- `Acp.Net.Process.0.1.0-alpha.1.nupkg`
- `Acp.Net.Testing.0.1.0-alpha.1.nupkg`

## Karar Etkisi

Bu noktada Acp.Net Process/Testing hattinin teknik riski daha da dustu.

Bir sonraki is artik yeni ozellik eklemek degil, alpha hazirlik checklist'i olmali:

1. README ve XML docs son polish.
2. License/package metadata karari.
3. Git repo/branch stratejisi.
4. NuGet publish edilmeyecekse local package feed denemesi.
5. Bir gercek ACP agent ile dogfooding.
