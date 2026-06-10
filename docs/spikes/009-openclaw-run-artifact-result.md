# Spike 009 Sonuc Raporu: OpenClaw-Oriented Run Artifact

Tarih: 2026-06-09

## Ozet

Spike basarili.

Runner artik transcript disinda makine tarafindan okunabilir bir run artifact JSON dosyasi uretebiliyor. Bu dosya OpenClaw tarafinin subagent run sonucunu hizli degerlendirmesi icin tasarlandi.

## Eklenen API

`AcpProcessOptions`:

```csharp
public string? AgentName { get; init; }
public string? RunArtifactPath { get; init; }
```

Artifact modeli:

```csharp
public sealed record AcpRunArtifact(
    string RunId,
    string AgentName,
    string? WorkingDirectory,
    bool UsesWsl,
    string ResolvedCommandLine,
    string Result,
    AcpRunFailureKind FailureKind,
    string? FailureMessage,
    string? TranscriptPath,
    IReadOnlyList<AcpExecutablePreflightResult> Preflight,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt);
```

## Artifact Ornegi

Gercek sample run sonucu:

```json
{
  "agentName": "openclaw-fake-acp-subagent",
  "usesWsl": true,
  "result": "completed",
  "failureKind": "None",
  "preflight": [
    {
      "name": "python3",
      "found": true,
      "path": "/usr/bin/python3",
      "missingPolicy": "Throw",
      "isFailure": false
    },
    {
      "name": "rg",
      "found": false,
      "error": "which exited 1",
      "missingPolicy": "Warn",
      "isFailure": false
    }
  ]
}
```

## Neden Onemli?

Transcript detayli kanit dosyasi olarak kalir. Run artifact ise orkestrator icin hizli sonuc sozlesmesidir.

OpenClaw gibi bir sistem sunlari transcript parse etmeden gorebilir:

- run basarili mi?
- environment failure mi?
- hangi critical tool eksik?
- agent WSL icinde mi calisti?
- asil transcript nerede?
- resolved command line neydi?

## Karar Etkisi

Bu spike, Acp.Net'in "subagent runtime substrate" pozisyonunu guclendirdi.

OpenClaw entegrasyonu icin dogrudan kullanilabilecek ilk contract ortaya cikti.

