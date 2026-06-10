# Spike 008 Sonuc Raporu: Environment Policy + OpenClaw Failure Contract

Tarih: 2026-06-09

## Ozet

Spike basarili.

`RequiredExecutables = ["tool"]` geriye uyumlu olarak warning davranisini korudu. Yeni `RequiredTools` API'si ile her tool icin eksik oldugunda `Warn` veya `Throw` politikasi tanimlanabiliyor.

## Eklenen API

```csharp
public enum AcpMissingExecutablePolicy
{
    Warn,
    Throw
}

public sealed record AcpRequiredExecutable(string Name, AcpMissingExecutablePolicy MissingPolicy)
{
    public static AcpRequiredExecutable Warn(string name);
    public static AcpRequiredExecutable Throw(string name);
}
```

`AcpProcessOptions`:

```csharp
public string? AgentName { get; init; }
public string? RunArtifactPath { get; init; }
public IReadOnlyList<AcpRequiredExecutable> RequiredTools { get; init; }
```

`AcpExecutablePreflightResult` artik sunlari da tasir:

```csharp
AcpMissingExecutablePolicy MissingPolicy
bool IsFailure
```

Eksik `Throw` tool icin runner agent process'i baslatmadan `AcpPreflightException` firlatir.

## Failure Contract

Ilk failure ayrimi eklendi:

```csharp
public enum AcpRunFailureKind
{
    None,
    EnvironmentFailure,
    ProcessFailure,
    ProtocolFailure,
    AgentFailure,
    Unknown
}
```

Bu spike kapsaminda aktif kullanilan ayrim:

- `EnvironmentFailure`: fail-fast preflight.
- `ProcessFailure`: process baslatma/kapatma/exit problemi.
- `None`: run basarili tamamlandi.

## Dogrulanan Davranis

Eksik optional tool:

```csharp
RequiredTools = [AcpRequiredExecutable.Warn("rg")]
```

Sonuc:

- `preflight.tool.missing`
- `IsFailure = false`
- agent calisir.

Eksik critical tool:

```csharp
RequiredTools = [AcpRequiredExecutable.Throw("git")]
```

Sonuc:

- `preflight.tool.missing`
- `preflight.failed`
- `AcpPreflightException`
- agent process baslamaz.
- run artifact `failureKind = EnvironmentFailure` yazar.

## Test Sonucu

Unit test sayisi 13'ten 14'e cikti.

Integration test sayisi 2'den 3'e cikti.

Son komut:

```bash
dotnet test '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\acp-net\AcpNetMvp.slnx' --logger 'console;verbosity=minimal'
```

Sonuc:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
```

## Karar Etkisi

Bu spike, Acp.Net'in OpenClaw gibi bir orkestrator icin kullanilabilir runtime contract uretmesini sagladi.

Onemli ayrim:

Agent basarisizligi ile environment basarisizligi artik ayni sey degil.

