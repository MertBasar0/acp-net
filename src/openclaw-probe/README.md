# OpenClaw Acp.Net Probe (Node Drafts)

🇹🇷 Türkçe sürüm: [README.tr.md](README.tr.md)

This folder contains small Node-side drafts for OpenClaw integration. They intentionally live inside the Acp.Net repository instead of modifying the OpenClaw core repository.

## Command Wrapper Probe

`openclaw-acpnet-probe.mjs` mirrors what an OpenClaw command/tool wrapper would need to do:

1. run the Acp.Net OpenClaw-style subagent sample,
2. parse the sample stdout result,
3. read the Acp.Net run artifact,
4. return one compact OpenClaw-oriented JSON result.

Run:

```bash
node src/openclaw-probe/openclaw-acpnet-probe.mjs
```

Output contract:

```json
{
  "kind": "openclaw.acpnet.probe.result",
  "ok": true,
  "result": "completed",
  "failureKind": "None",
  "agentName": "openclaw-fake-acp-subagent",
  "usesWsl": true,
  "preflight": {
    "criticalMissing": [],
    "warnings": []
  }
}
```

This probe uses the deterministic fake ACP agent. It does not use Gemini or Claude quota.

## Doctor Adapter Draft

`doctor-adapter-draft.mjs` maps the probe result to the OpenClaw doctor/lint surfaces described in [docs/contracts/openclaw-doctor-adapter-draft.md](../../docs/contracts/openclaw-doctor-adapter-draft.md).

Verify it against the scenario fixtures:

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

Expected:

```text
doctor adapter scenarios ok (4)
```
