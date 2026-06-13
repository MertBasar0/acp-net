# OpenClaw Acp.Net Probe Command

🇹🇷 Türkçe sürüm: [README.tr.md](README.tr.md)

This sample is the stabilized diagnostic command probe.

It acts like a command that an OpenClaw plugin/tool wrapper could execute:

1. start a deterministic ACP subagent through `AcpProcessRunner`,
2. enforce critical/optional preflight policy,
3. run one ACP prompt,
4. write the Acp.Net transcript and run artifact,
5. print one OpenClaw-oriented JSON result to stdout.

By default it does not call any LLM or model API.

Run:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj
```

Useful options:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj -- \
  --agent my-agent \
  --cwd /path/to/workspace \
  --required-tool git \
  --optional-tool rg \
  --artifact-dir artifacts/openclaw-probe
```

Custom ACP agent command:

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj -- \
  --agent gemini \
  --command gemini \
  --arg --acp \
  --arg --skip-trust \
  --arg --approval-mode \
  --arg plan \
  --runtime wsl \
  --required-tool git \
  --optional-tool rg
```

Exit codes:

- `0`: `ok=true`
- `2`: environment/preflight failure
- `3`: runtime/protocol/agent/unknown failure
- `64`: invalid CLI configuration

Stdout is reserved for one JSON result. Help text and diagnostics go to stderr.
