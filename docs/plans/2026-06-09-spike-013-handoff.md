# 2026-06-09 Spike 013 Handoff

## Tamamlanan Is

Spike 013 tamamlandi:

**Acp.Net Diagnostic Command Stabilization**

## Degisen Ana Dosya

`src/samples/openclaw-acpnet-probe/Program.cs`

## Yeni CLI Contract

Probe artik su argumanlari destekliyor:

- `--agent`
- `--cwd`
- `--command`
- `--arg`
- `--required-tool`
- `--optional-tool`
- `--require-command-executable`
- `--transcript`
- `--artifact`
- `--artifact-dir`
- `--prompt`
- `--runtime`
- `--wsl-distribution`
- `--timeout-seconds`
- `--shutdown-ms`

Exit codes:

```text
0   ok=true
2   environment/preflight failure
3   runtime/protocol/agent/unknown failure
64  invalid CLI configuration
```

## Son Dogrulamalar

Default fake-agent probe:

```text
exitCode=0
ok=true
result=completed
failureKind=None
```

Missing critical tool:

```text
exitCode=2
failureKind=EnvironmentFailure
```

Invalid config:

```text
exitCode=64
failureKind=ConfigurationFailure
```

Explicit artifact dir:

```text
exitCode=0
agentName=spike-013-fake
```

Ana test suite:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
```

Pack:

```text
Acp.Net.Process: ok
Acp.Net.Testing: ok
```

## Sonraki Oneri

Spike 014:

**OpenClaw Doctor Adapter Draft**

Hedef OpenClaw core'a hemen kod eklemek degil; once Acp.Net diagnostic command sonucunun OpenClaw doctor/tool sonucuna nasil map edilecegini yazili contract haline getirmek.

