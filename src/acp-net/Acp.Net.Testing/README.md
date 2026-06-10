# Acp.Net.Testing

Fake ACP agent and transcript assertion helpers for .NET ACP integration tests.

## Install

```bash
dotnet add package Acp.Net.Testing --prerelease
```

## Fake Agent

```csharp
using AcpNet.Testing;

var artifactDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts");
var agentPath = FakeAcpAgentScript.WriteDefault(artifactDir);
```

`WriteDefault` creates a small stdio ACP agent that supports:

- `initialize`
- `session/new`
- `session/prompt`
- streaming `session/update`
- `session/cancel`

## Transcript Assertions

```csharp
AcpTranscriptAssert.ExistsAndNotEmpty("agent-transcript.ndjson");
AcpTranscriptAssert.Contains("agent-transcript.ndjson", "session/update");
```

Use this package with `Acp.Net.Process` and a protocol package such as `AgentClientProtocol`.
