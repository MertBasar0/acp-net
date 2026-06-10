# Spike 013 Sonuc Raporu: Acp.Net Diagnostic Command Stabilization

Tarih: 2026-06-09

## Ozet

Spike basarili.

`openclaw-acpnet-probe` sample'i sabit fake-agent demodan daha stabil bir diagnostic command contract'ina tasindi.

Hedef:

> OpenClaw core'a dokunmadan, OpenClaw'in external command/tool olarak cagirabilecegi tek JSON stdout ureten Acp.Net diagnostic probe saglamak.

## Guncellenen Sample

Konum:

`src/samples/openclaw-acpnet-probe/`

Varsayilan davranis:

- fake ACP agent kullanir,
- model kotasi harcamaz,
- `python3` critical preflight yapar,
- `rg` optional warning preflight yapar,
- transcript ve run artifact uretir,
- stdout'a tek JSON result basar.

## Eklenen CLI Argumanlari

```text
--agent <name>
--cwd <path>
--command <command>
--arg <value>
--required-tool <name>
--optional-tool <name>
--require-command-executable
--transcript <path>
--artifact <path>
--artifact-dir <path>
--prompt <text>
--runtime <auto|native|wsl>
--wsl-distribution <name>
--timeout-seconds <seconds>
--shutdown-ms <ms>
--help
```

`--command` verilmezse deterministic fake ACP agent kullanilir.

`--command` verilirse probe o ACP-compatible command'i calistirmaya calisir.

## Exit Code Contract

```text
0   ok=true
2   environment/preflight failure
3   runtime/protocol/agent/unknown failure
64  invalid CLI configuration
```

Stdout contract:

- normal ve hata durumunda tek JSON result,
- help disinda stdout'a baska yazi basilmamali.

Stderr contract:

- help ve diagnostic metinler icin ayrildi.

## Dogrulanan Senaryolar

### Basarili Default Fake Agent

Komut:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\openclaw-acpnet-probe\openclaw-acpnet-probe.csproj'
```

Sonuc:

```text
exitCode=0
ok=true
result=completed
failureKind=None
sessionId=fake-session-1
stopReason=EndTurn
```

### Environment Fail-Fast

Komut:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\openclaw-acpnet-probe\openclaw-acpnet-probe.csproj' -- --required-tool definitely-not-a-real-acpnet-critical-tool
```

Sonuc:

```text
exitCode=2
ok=false
failureKind=EnvironmentFailure
criticalMissing=definitely-not-a-real-acpnet-critical-tool
```

### Configuration Failure

Komut:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\openclaw-acpnet-probe\openclaw-acpnet-probe.csproj' -- --arg orphan
```

Sonuc:

```text
exitCode=64
ok=false
failureKind=ConfigurationFailure
failureMessage=--arg requires --command.
```

### Explicit Artifact Directory

Komut:

```bash
dotnet run --project '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\src\samples\openclaw-acpnet-probe\openclaw-acpnet-probe.csproj' -- --artifact-dir '\\wsl.localhost\Ubuntu\home\mertb\.openclaw\workspace\acp-net-training-factory\artifacts\spike-013-explicit' --agent spike-013-fake --prompt 'OpenClaw probe explicit path test.'
```

Sonuc:

```text
exitCode=0
agentName=spike-013-fake
runArtifactPath=...\artifacts\spike-013-explicit\probe-run.json
transcriptPath=...\artifacts\spike-013-explicit\probe-transcript.ndjson
```

## Test ve Pack

Ana test suite:

```text
Acp.Net.UnitTests: 14 passed
Acp.Net.IntegrationTests: 3 passed
```

Pack:

- `Acp.Net.Process` exit code 0
- `Acp.Net.Testing` exit code 0

## Karar Etkisi

Bu spike, Acp.Net'in OpenClaw'a en dusuk riskli entegrasyon yolunu somutlastirdi:

> OpenClaw, Acp.Net'i once external diagnostic command olarak cagirabilir.

Bu yol ACPX runtime replacement degil. Bunun yerine OpenClaw'a environment/preflight/failure evidence getirir.

## Sonraki Mantikli Is

Spike 014 onerisi:

**OpenClaw Doctor Adapter Draft**

Amac:

1. OpenClaw tarafinda kod degistirmeden once doctor/tool adapter pseudo-contract'i yaz.
2. `openclaw-acpnet-probe` JSON sonucunun OpenClaw doctor sonucuna nasil map edilecegini belirle.
3. `EnvironmentFailure`, `ConfigurationFailure`, `ProcessFailure` icin kullaniciya donulecek mesajlari tasarla.
4. Node child process -> Windows interop kisitini nasil asacagimiza karar ver:
   - C# command'i Windows host tarafindan calistirma,
   - WSL icinde native dotnet kurma,
   - OpenClaw sandbox disindan approved command olarak calistirma.

