# OpenClaw Subagent Runner Sample

🇹🇷 Türkçe sürüm: [README.tr.md](README.tr.md)

This sample simulates the smallest useful OpenClaw integration shape:

1. Start an ACP-compatible subagent process.
2. Preflight required runtime tools.
3. Send one ACP prompt.
4. Save a transcript and a machine-readable run artifact.
5. Return a compact result object to the caller.

It intentionally uses `FakeAcpAgentScript` instead of a real LLM agent so that the sample is deterministic and does not spend model quota.

Run:

```bash
dotnet run --project src/samples/openclaw-subagent-runner/openclaw-subagent-runner.csproj
```
