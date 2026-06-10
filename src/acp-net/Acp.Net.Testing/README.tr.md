# Acp.Net.Testing

.NET ACP entegrasyon testleri için sahte ACP agent ve transcript doğrulama yardımcıları.

🇬🇧 English version: [README.md](README.md)

## Kurulum

```bash
dotnet add package Acp.Net.Testing --prerelease
```

## Sahte Agent

```csharp
using AcpNet.Testing;

var artifactDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts");
var agentPath = FakeAcpAgentScript.WriteDefault(artifactDir);
```

`WriteDefault`, şunları destekleyen küçük bir stdio ACP agent'ı oluşturur:

- `initialize`
- `session/new`
- `session/prompt`
- streaming `session/update`
- `session/cancel`

## Transcript Doğrulamaları

```csharp
AcpTranscriptAssert.ExistsAndNotEmpty("agent-transcript.ndjson");
AcpTranscriptAssert.Contains("agent-transcript.ndjson", "session/update");
```

Bu paketi `Acp.Net.Process` ve `AgentClientProtocol` gibi bir protokol paketiyle birlikte kullanın.
