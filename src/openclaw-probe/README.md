# OpenClaw Acp.Net Probe

This is a small command-wrapper probe for Spike 011.

It intentionally lives inside the Acp.Net workspace instead of modifying the OpenClaw core repository. The shape mirrors what an OpenClaw command/tool wrapper would need to do:

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
