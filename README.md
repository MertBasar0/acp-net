# Acp.Net

> 🇹🇷 Bu dokümanın Türkçe sürümü: [README.tr.md](README.tr.md)

Acp.Net is a .NET product family for running, testing, and diagnosing ACP-compatible agent processes.

It is **not** a full ACP protocol SDK and it is not an OpenClaw core fork. The current product direction is:

> Keep Acp.Net as an independent package family. Use OpenClaw as a reference consumer and dogfood environment, not as the product boundary.

## What It Provides

Current implemented surfaces:

- `Acp.Net.Process`: process runner, WSL/native runtime bridge, path mapping, environment shaping, preflight checks, transcript recording, run artifacts, shutdown policy.
- `Acp.Net.Testing`: deterministic fake ACP agent scripts and transcript assertions for integration tests.
- Diagnostic samples/tools: OpenClaw-oriented probe command and doctor/lint mapping draft. These are intentionally not a separate NuGet package yet.

The core value is separating these failure classes:

- environment failure: missing tools, wrong PATH, wrong runtime, WSL/path issue
- process failure: launch, exit, timeout, shutdown
- protocol failure: ACP/JSON-RPC flow issue
- agent failure: the agent ran but failed the delegated task

## Relationship To ACP Protocol Packages

Acp.Net does not own ACP protocol types, so it is protocol-package-agnostic: it produces the agent's stdio streams and hands them to whichever ACP protocol/JSON-RPC package you prefer — for example [`AgentClientProtocol`](https://www.nuget.org/packages/AgentClientProtocol), [`dotacp`](https://www.nuget.org/packages/dotacp.protocol), or [`LibAcp`](https://www.nuget.org/packages/LibAcp). Those packages model the protocol and run JSON-RPC over a stream the caller provides; they each leave process launch and platform interop to the consumer. Acp.Net is that layer:

- starting the agent process
- shaping environment variables and PATH
- mapping Windows/WSL paths
- checking required tools before launch
- recording raw stdio and lifecycle events
- producing machine-readable run artifacts
- testing process-boundary behavior with fake agents

The samples use `AgentClientProtocol`, but nothing in `Acp.Net.Process` depends on it. See [docs/ECOSYSTEM.md](docs/ECOSYSTEM.md) for how Acp.Net sits next to the other .NET ACP packages.

## Repository Layout

- `src/acp-net/Acp.Net.Process/`: production runtime package.
- `src/acp-net/Acp.Net.Testing/`: testing helpers.
- `src/acp-net/Acp.Net.UnitTests/`: unit tests.
- `src/acp-net/Acp.Net.IntegrationTests/`: process-boundary integration tests.
- `src/samples/`: sample consumers and probes.
- `src/openclaw-probe/`: OpenClaw-oriented diagnostic/doctor adapter drafts.
- `docs/decisions/`: ADRs and product decisions.
- `docs/product/`: product design notes.
- `docs/contracts/`: JSON/result contracts and adapter mapping fixtures.

Dated spike reports and day-to-day handoff notes live in the untracked `notes/` folder at the repository root, which is ignored by git and never pushed to the remote. The durable outcomes of that work are recorded in `docs/decisions/`.

## Quick Verification

From the repository root:

```bash
dotnet test src/acp-net/AcpNetMvp.slnx --logger "console;verbosity=minimal"
```

All unit and integration tests should pass.

Run the diagnostic probe without spending model quota (uses a deterministic fake ACP agent; requires `python3` in the agent runtime):

```bash
dotnet run --project src/samples/openclaw-acpnet-probe/openclaw-acpnet-probe.csproj
```

The probe prints a single JSON result to stdout and exits `0` on success.

Validate the OpenClaw doctor adapter draft (requires Node.js):

```bash
node src/openclaw-probe/verify-doctor-adapter-draft.mjs
```

Expected:

```text
doctor adapter scenarios ok (4)
```

> **Windows + WSL note:** if you run a Windows `dotnet.exe` against project files that live inside a WSL filesystem, pass them as UNC paths (`\\wsl.localhost\<Distro>\...`). See [docs/DEVELOPMENT_GUIDE.md](docs/DEVELOPMENT_GUIDE.md) for details.

## Documentation

Key documents:

- [Current Status](docs/CURRENT_STATUS.md) — single source of truth for project state
- [Roadmap](docs/ROADMAP.md)
- [Development Guide](docs/DEVELOPMENT_GUIDE.md)
- [OpenClaw Integration Strategy](docs/OPENCLAW_INTEGRATION_STRATEGY.md)
- [Release Checklist](docs/RELEASE_CHECKLIST.md)
- [Product Design](docs/product/acp-net-mvp-product-design.md)
- [Ecosystem Positioning](docs/ECOSYSTEM.md) — how Acp.Net sits next to other .NET ACP packages
- [OpenClaw Integration (reference)](docs/integrations/openclaw.md) — calling the diagnostic probe from an OpenClaw health check
- [Decisions (ADRs)](docs/decisions/)

All core documents have full Turkish versions next to them with a `.tr.md` suffix.

## Near-Term Direction

The next recommended work is not an OpenClaw core PR. The next work should harden Acp.Net as an independent package family:

1. stabilize package API boundaries,
2. keep diagnostics as repository tooling until the command contract has more usage evidence,
3. improve docs and examples,
4. prepare first alpha NuGet packages,
5. keep OpenClaw integration as reference/dogfood material.

## License

Acp.Net is licensed under the Apache License 2.0. See [LICENSE](LICENSE).
